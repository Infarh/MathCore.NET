using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using MathCore.NET.HTTP.Events;
// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.NET.HTTP
{
    public class WebServer
    {
        public static bool AddFirewallRule(int port = 80)
        {
            var cmd_info = new ProcessStartInfo(
                    "netsh",
                    $"advfirewall firewall add rule name=\"Web{port}\" dir=in action=allow protocol=TCP localport={port}")
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

        public static bool RemoveFirewallRule(int port = 80)
        {
            var cmd_info = new ProcessStartInfo(
                "netsh",
                $"advfirewall firewall delete rule name=\"Web{port}\" dir=in protocol=TCP localport={port}")
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
                if(context != null)
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
