using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using MathCore.NET.HTTP.Events;
// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.NET.HTTP
{
    public class WebServer
    {
        public static bool AddFirewallRule(int Port = 80)
        {
            var cmd_info = new ProcessStartInfo(
                    "netsh",
                    $"advfirewall firewall add rule name=\"Web{Port}\" dir=in action=allow protocol=TCP localport={Port}")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = true
            };
            var process = new Process { StartInfo = cmd_info };
            process.Start();
            //var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var exit_code = process.ExitCode;
            return exit_code == 0;
        }

        public static bool RemoveFirewallRule(int Port = 80)
        {
            var cmd_info = new ProcessStartInfo(
                "netsh",
                $"advfirewall firewall delete rule name=\"Web{Port}\" dir=in protocol=TCP localport={Port}")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = true
            };
            var process = Process.Start(cmd_info) ?? throw new InvalidOperationException();
            var exit_code = process.ExitCode;
            return exit_code == 0;
        }

        public static bool AddUrlAclRule(int Port = 80, string UserName = null)
        {
            var cmd_info = new ProcessStartInfo(
                "netsh",
                $"http add urlacl url=http://+:{Port}/ user={UserName ?? Environment.UserName}")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = true
            };
            var process = new Process { StartInfo = cmd_info };
            process.Start();
            //var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var exit_code = process.ExitCode;
            return exit_code == 0;
        }

        public readonly struct AclRule
        {
            public readonly struct UserAccess
            {
                public string User { get; }

                public bool CanListen { get; }

                public bool CanDelegate { get; }

                internal UserAccess(IReadOnlyList<KeyValuePair<string, string>> Values, ref int Index)
                {
                    User = Values[Index].Value;
                    CanListen = Values[Index + 1].Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                    CanDelegate = Values[Index + 2].Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                    Index += 3;
                }

                public override string ToString() => $"{User}:listen={CanListen};delegate={CanDelegate}";
            }

            public readonly struct SDDLInfo
            {
                public SDDLInfo(string info)
                {
                    var match = Regex.Match(info, @"D\:(?<info>\((?<access>[AD]);;(?<value>[A-Z]+);;;(?<sid>[^)]+)\))+");
                    if(!match.Success)
                        throw new FormatException("ошибка формата записи токена безопасности");
                }
            }

            public string Uri { get; }

            public string SDDL { get; }

            public IReadOnlyList<UserAccess> Access { get; }

            public AclRule(IReadOnlyList<KeyValuePair<string, string>> Values)
            {
                Uri = Values[0].Value;
                var index = 1;
                var access = new List<UserAccess>();
                while (!Values[index].Key.Equals("SDDL", StringComparison.OrdinalIgnoreCase)) 
                    access.Add(new UserAccess(Values, ref index));
                Access = access;
                SDDL = Values[Values.Count - 1].Value;
            }

            public override string ToString() => $"{Uri} {string.Join(", ", Access)}";
        }

        public static IEnumerable<AclRule> GetRules()
        {
            var cmd_info = new ProcessStartInfo("netsh", "http show urlacl")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = new Process { StartInfo = cmd_info };
            process.Start();

            using var reader = process.StandardOutput;
            for (var i = 0; i < 4 && !reader.EndOfStream; i++)
                reader.ReadLine();

            List<KeyValuePair<string, string>> rule = null;
            while (!reader.EndOfStream)
            {
                static KeyValuePair<string, string> GetValue(string Line)
                {
                    var separator_index = Line.IndexOf(':');
                    if (separator_index < 0) return new KeyValuePair<string, string>("value", Line.Trim());
                    var key = Line.Substring(0, separator_index).Trim();
                    var value = Line.Substring(separator_index + 1, Line.Length - separator_index - 1).Trim();
                    return new KeyValuePair<string, string>(key, value);
                }

                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    if (rule != null)
                    {
                        if (rule.Count > 0)
                            yield return new AclRule(rule);
                        rule = null;
                        continue;
                    }
                if (rule is null) rule = new List<KeyValuePair<string, string>>();
                rule.Add(GetValue(line));
            }

            if (rule != null && rule.Count >= 5)
                yield return new AclRule(rule);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Url">http://+:8080 http://*:8080/Server</param>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static bool AddUrlAclRule(string Url, string UserName = null)
        {
            var cmd_info = new ProcessStartInfo(
                "netsh",
                $"http add urlacl url={Url} user={UserName ?? Environment.UserName}")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = true
            };
            var process = new Process { StartInfo = cmd_info };
            process.Start();
            //var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var exit_code = process.ExitCode;
            return exit_code == 0;
        }

        public static bool RemoveUrlAclRule(int Port = 80)
        {
            var cmd_info = new ProcessStartInfo(
                "netsh",
                $"http delete urlacl url=http://+:{Port}/")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = true
            };
            var process = new Process { StartInfo = cmd_info };
            process.Start();
            //var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var exit_code = process.ExitCode;
            return exit_code == 0;
        }

        public static bool RemoveUrlAclRule(string Url)
        {
            var cmd_info = new ProcessStartInfo(
                "netsh",
                $"http delete urlacl url={Url}")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                RedirectStandardOutput = true,
                UseShellExecute = true
            };
            var process = new Process { StartInfo = cmd_info };
            process.Start();
            //var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var exit_code = process.ExitCode;
            return exit_code == 0;
        }

        /* -------------------------------------------------------------------------------------------------------------------------------- */

        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        /* -------------------------------------------------------------------------------------------------------------------------------- */

        private bool _Enabled;
        // ReSharper disable CommentTypo
        private readonly int _Port; // netsh http add urlacl url=http://+:8989/ user=shmac
        // ReSharper restore CommentTypo
        private readonly object _SyncRoot = new object();
        private HttpListener _Listener;
        private readonly RouteManager _RouteManager;
        private DirectoryInfo _HomeDirectory = new DirectoryInfo(Environment.CurrentDirectory);

        /* -------------------------------------------------------------------------------------------------------------------------------- */

        public DirectoryInfo HomeDirectory
        {
            get => _HomeDirectory;
            set
            {
                if (!value.Exists) throw new ArgumentException("Директория не существует");
                _HomeDirectory = value;
            }
        }

        public string HomeDirectoryPath
        {
            get => HomeDirectory.FullName;
            set => HomeDirectory = new DirectoryInfo(value);
        }

        public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }

        public int Port => _Port;

        public RouteManager Routes => _RouteManager;

        /* -------------------------------------------------------------------------------------------------------------------------------- */

        public WebServer(int port = 80)
        {
            _Port = port;
            _RouteManager = new RouteManager(this);
        }

        /* -------------------------------------------------------------------------------------------------------------------------------- */

        public void Restart()
        {
            lock (_SyncRoot)
            {
                Stop();
                Start();
            }
        }

        public void Start()
        {
            if (_Enabled) return;
            lock (_SyncRoot)
            {
                if (_Enabled) return;
                _Listener = new HttpListener();
                _Listener.Prefixes.Add($"http://+:{_Port}/");
                _Listener.Prefixes.Add($"http://*:{_Port}/");
                _Enabled = true;
                ListenAsync();
            }
        }

        public void Stop()
        {
            if (!_Enabled) return;
            lock (_SyncRoot)
            {
                if (!_Enabled) return;
                _Listener = null;

                _Enabled = false;
            }
        }

        /* -------------------------------------------------------------------------------------------------------------------------------- */

        private async void ListenAsync()
        {
            var listener = _Listener;
            listener.Start();
            HttpListenerContext context = null;
            while (_Enabled)
            {
                var receive_context_task = listener.GetContextAsync();
                if (context != null)
                    ProcessRequest(context);
                context = await receive_context_task.ConfigureAwait(false);
            }
            listener.Stop();
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            RequestReceived?.Invoke(this, new RequestReceivedEventArgs(context));
            _RouteManager?.Process(context);
        }

        /* -------------------------------------------------------------------------------------------------------------------------------- */
    }
}
