using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MathCore.NET.HTTP.Events;
// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.NET.HTTP;

/// <summary>HTTP веб-сервер на основе HttpListener с поддержкой маршрутизации запросов</summary>
public class WebServer
{
    /// <summary>Добавить правило брандмауэра Windows для разрешения входящих подключений на указанный порт</summary>
    /// <param name="Port">Номер порта (по умолчанию 80)</param>
    /// <returns>true если правило успешно добавлено, иначе false</returns>
    /// <exception cref="InvalidOperationException">Если не удалось запустить процесс netsh</exception>
    public static bool AddFirewallRule(int Port = 80)
    {
        var cmd_info = new ProcessStartInfo(
                "netsh",
                $"advfirewall firewall add rule name=\"Web{Port}\" dir=in action=allow protocol=TCP localport={Port}")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Verb = "runas",
            UseShellExecute = false
        };
        using var process = new Process { StartInfo = cmd_info };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>Удалить правило брандмауэра Windows для указанного порта</summary>
    /// <param name="Port">Номер порта (по умолчанию 80)</param>
    /// <returns>true если правило успешно удалено, иначе false</returns>
    /// <exception cref="InvalidOperationException">Если не удалось запустить процесс netsh</exception>
    public static bool RemoveFirewallRule(int Port = 80)
    {
        var cmd_info = new ProcessStartInfo(
            "netsh",
            $"advfirewall firewall delete rule name=\"Web{Port}\" dir=in protocol=TCP localport={Port}")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Verb = "runas",
            UseShellExecute = false
        };
        using var process = Process.Start(cmd_info) ?? throw new InvalidOperationException("Не удалось запустить процесс netsh");
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>Добавить правило списка управления доступом (ACL) для URL на указанном порту</summary>
    /// <param name="Port">Номер порта (по умолчанию 80)</param>
    /// <param name="UserName">Имя пользователя для предоставления доступа, если null используется текущий пользователь</param>
    /// <returns>true если правило успешно добавлено, иначе false</returns>
    public static bool AddUrlAclRule(int Port = 80, string? UserName = null)
    {
        var cmd_info = new ProcessStartInfo(
            "netsh",
            $"http add urlacl url=http://+:{Port}/ user={UserName ?? Environment.UserName}")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Verb = "runas",
            UseShellExecute = false
        };
        using var process = new Process { StartInfo = cmd_info };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>Информация о правиле списка управления доступом (ACL) для URL с данными пользователей и токеном безопасности</summary>
    public readonly struct AclRule
    {
        /// <summary>Информация о доступе конкретного пользователя к URL</summary>
        public readonly struct UserAccess
        {
            /// <summary>Имя пользователя</summary>
            public string User { get; }

            /// <summary>Может ли пользователь прослушивать URL</summary>
            public bool CanListen { get; }

            /// <summary>Может ли пользователь делегировать права на прослушивание</summary>
            public bool CanDelegate { get; }

            /// <summary>Инициализирует новый экземпляр структуры UserAccess на основе списка пар ключ-значение</summary>
            /// <param name="Values">Список пар ключ-значение с данными пользователя</param>
            /// <param name="Index">Индекс начала данных пользователя, после инициализации увеличивается на 3</param>
            /// <exception cref="ArgumentNullException">Если Values равен null</exception>
            /// <exception cref="ArgumentException">Если недостаточно элементов для инициализации</exception>
            internal UserAccess(IReadOnlyList<KeyValuePair<string, string>>? Values, ref int Index)
            {
                if (Values == null)
                    throw new ArgumentNullException(nameof(Values));
                if (Index + 2 >= Values.Count)
                    throw new ArgumentException("Недостаточно элементов в списке значений");

                User = Values[Index].Value;
                CanListen = Values[Index + 1].Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                CanDelegate = Values[Index + 2].Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                Index += 3;
            }

            public override string ToString() => $"{User}:listen={CanListen};delegate={CanDelegate}";
        }

        /// <summary>Информация о токене безопасности (SDDL) для контроля доступа к URL</summary>
        public readonly struct SDDLInfo
        {
            /// <summary>Инициализирует новый экземпляр структуры SDDLInfo на основе строки SDDL</summary>
            /// <param name="info">Строка формата SDDL (Security Descriptor Definition Language)</param>
            /// <exception cref="FormatException">Если формат строки SDDL некорректен</exception>
            public SDDLInfo(string info)
            {
                var match = Regex.Match(info, @"D\:(?<info>\((?<access>[AD]);;(?<value>[A-Z]+);;;(?<sid>[^)]+)\))+");
                if (!match.Success)
                    throw new FormatException("Ошибка формата записи токена безопасности");
            }
        }

        /// <summary>URI для которого установлено правило доступа</summary>
        public string Uri { get; }

        /// <summary>Дескриптор безопасности в формате SDDL</summary>
        public string SDDL { get; }

        /// <summary>Список пользователей и их прав доступа к URL</summary>
        public IReadOnlyList<UserAccess> Access { get; }

        /// <summary>Инициализирует новый экземпляр структуры AclRule на основе списка пар ключ-значение</summary>
        /// <param name="Values">Список пар ключ-значение с данными правила доступа</param>
        /// <exception cref="ArgumentNullException">Если Values равен null</exception>
        /// <exception cref="ArgumentException">Если недостаточно значений в списке</exception>
        public AclRule(IReadOnlyList<KeyValuePair<string, string>>? Values)
        {
            if (Values == null)
                throw new ArgumentNullException(nameof(Values));
            if (Values.Count < 2)
                throw new ArgumentException("Недостаточно значений для создания AclRule");

            Uri = Values[0].Value;
            var index = 1;
            var access = new List<UserAccess>();
            while (index < Values.Count && !Values[index].Key.Equals("SDDL", StringComparison.OrdinalIgnoreCase))
                access.Add(new(Values, ref index));
            Access = access;
            SDDL = Values[Values.Count - 1].Value;
        }

        public override string ToString() => $"{Uri} {string.Join(", ", Access)}";
    }

    /// <summary>Получить все текущие правила списка управления доступом (ACL) для URL из системы</summary>
    /// <returns>Перечисление правил доступа, установленных в операционной системе</returns>
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

        using var process = new Process { StartInfo = cmd_info };
        process.Start();

        using var reader = process.StandardOutput;
        for (var i = 0; i < 4 && !reader.EndOfStream; i++)
            reader.ReadLine();

        List<KeyValuePair<string, string>>? rule = null;
        while (!reader.EndOfStream)
        {
            static KeyValuePair<string, string> GetValue(string Line)
            {
                var separator_index = Line.IndexOf(':');
                if (separator_index < 0) return new("value", Line.Trim());
                var key = Line.Substring(0, separator_index).Trim();
                var value = Line.Substring(separator_index + 1, Line.Length - separator_index - 1).Trim();
                return new(key, value);
            }

            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                if (rule != null)
                {
                    if (rule.Count > 0)
                        yield return new(rule);
                    rule = null;
                    continue;
                }
            rule ??= [];
            rule.Add(GetValue(line));
        }

        if (rule is { Count: >= 5 })
            yield return new(rule);
    }

    /// <summary>Добавить правило списка управления доступом (ACL) для конкретного URL</summary>
    /// <param name="Url">URL для которого добавляется правило доступа (например, http://+:8080 или http://*:8080/Server)</param>
    /// <param name="UserName">Имя пользователя для предоставления доступа, если null используется текущий пользователь</param>
    /// <returns>true если правило успешно добавлено, иначе false</returns>
    /// <exception cref="ArgumentException">Если Url пуст или содержит только пробелы</exception>
    public static bool AddUrlAclRule(string Url, string? UserName = null)
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentException("Url не может быть пустым", nameof(Url));

        var cmd_info = new ProcessStartInfo(
            "netsh",
            $"http add urlacl url={Url} user={UserName ?? Environment.UserName}")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Verb = "runas",
            UseShellExecute = false
        };
        using var process = new Process { StartInfo = cmd_info };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>Удалить правило списка управления доступом (ACL) для URL на указанном порту</summary>
    /// <param name="Port">Номер порта (по умолчанию 80)</param>
    /// <returns>true если правило успешно удалено, иначе false</returns>
    public static bool RemoveUrlAclRule(int Port = 80)
    {
        var cmd_info = new ProcessStartInfo(
            "netsh",
            $"http delete urlacl url=http://+:{Port}/")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Verb = "runas",
            UseShellExecute = false
        };
        using var process = new Process { StartInfo = cmd_info };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>Удалить правило списка управления доступом (ACL) для конкретного URL</summary>
    /// <param name="Url">URL для удаления правила доступа</param>
    /// <returns>true если правило успешно удалено, иначе false</returns>
    /// <exception cref="ArgumentException">Если Url пуст или содержит только пробелы</exception>
    public static bool RemoveUrlAclRule(string Url)
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentException("Url не может быть пустым", nameof(Url));

        var cmd_info = new ProcessStartInfo(
            "netsh",
            $"http delete urlacl url={Url}")
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Verb = "runas",
            UseShellExecute = false
        };
        using var process = new Process { StartInfo = cmd_info };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /* -------------------------------------------------------------------------------------------------------------------------------- */

    /// <summary>Возникает при получении HTTP запроса от клиента</summary>
    public event EventHandler<RequestReceivedEventArgs>? RequestReceived;

    /* -------------------------------------------------------------------------------------------------------------------------------- */

    private bool _Enabled;
    // ReSharper disable CommentTypo
    private readonly int _Port; // netsh http add urlacl url=http://+:8989/ user=shmac
    // ReSharper restore CommentTypo
    private readonly object _SyncRoot = new();
    private HttpListener _Listener = null!;
    private Task _ListenTask = null!;
    private readonly RouteManager _RouteManager = null!;
    private DirectoryInfo _HomeDirectory = new(Environment.CurrentDirectory);

    /* -------------------------------------------------------------------------------------------------------------------------------- */

    /// <summary>Каталог с публичными файлами сервера</summary>
    /// <exception cref="ArgumentException">При установке несуществующего каталога</exception>
    public DirectoryInfo HomeDirectory
    {
        get => _HomeDirectory;
        set
        {
            if (!value.Exists) throw new ArgumentException("Директория не существует");
            _HomeDirectory = value;
        }
    }

    /// <summary>Полный путь к домашнему каталогу сервера</summary>
    public string HomeDirectoryPath
    {
        get => HomeDirectory.FullName;
        set => HomeDirectory = new(value);
    }

    /// <summary>Включен ли веб-сервер</summary>
    public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }

    /// <summary>Номер порта на котором работает сервер</summary>
    public int Port => _Port;

    /// <summary>Менеджер маршрутов для обработки HTTP запросов</summary>
    public RouteManager Routes => _RouteManager;

    /* -------------------------------------------------------------------------------------------------------------------------------- */

    /// <summary>Инициализирует новый экземпляр WebServer с заданным портом</summary>
    /// <param name="port">Номер порта для прослушивания (по умолчанию 80)</param>
    public WebServer(int port = 80)
    {
        _Port = port;
        _RouteManager = new(this);
    }

    /* -------------------------------------------------------------------------------------------------------------------------------- */

    /// <summary>Перезагрузить сервер (остановить и запустить)</summary>
    public void Restart()
    {
        lock (_SyncRoot)
        {
            Stop();
            Start();
        }
    }

    /// <summary>Запустить веб-сервер и начать прослушивание входящих подключений</summary>
    public void Start()
    {
        if (_Enabled) return;
        lock (_SyncRoot)
        {
            if (_Enabled) return;
            _Listener = new();
            _Listener.Prefixes.Add($"http://+:{_Port}/");
            _Listener.Prefixes.Add($"http://*:{_Port}/");
            _Enabled = true;
            _ListenTask = ListenAsync();
        }
    }

    /// <summary>Остановить веб-сервер и закрыть все соединения</summary>
    public void Stop()
    {
        if (!_Enabled) return;
        lock (_SyncRoot)
        {
            if (!_Enabled) return;
            _Enabled = false;
            _Listener?.Close();
            _Listener = null!;
        }
    }

    /* -------------------------------------------------------------------------------------------------------------------------------- */

    private async Task ListenAsync()
    {
        var listener = _Listener;
        try
        {
            listener.Start();
            HttpListenerContext? context = null;
            while (_Enabled)
            {
                var receive_context_task = listener.GetContextAsync();
                if (context != null)
                    ProcessRequest(context);
                context = await receive_context_task.ConfigureAwait(false);
            }
        }
        catch (ObjectDisposedException) when (!_Enabled)
        {
            // Слушатель был закрыт через Stop()
        }
        catch (HttpListenerException) when (!_Enabled)
        {
            // Нормальное завершение при остановке
        }
        finally
        {
            listener?.Stop();
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        RequestReceived?.Invoke(this, new(context));
        _RouteManager?.Process(context);
    }

    /* -------------------------------------------------------------------------------------------------------------------------------- */
}