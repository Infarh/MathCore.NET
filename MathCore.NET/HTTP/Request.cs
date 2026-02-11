using System;
using System.IO;
using System.Linq;

namespace MathCore.NET.HTTP;

/// <summary>Представляет HTTP-запрос с методом, пути, параметров запроса и заголовков</summary>
public class Request : Message
{
    private string? _QueryString;

    private (string Key, string Value)[]? _QueryParameters;

    /// <summary>HTTP-метод запроса (GET, POST, PUT и т.д.)</summary>
    public string Method { get; set; } = null!;

    /// <summary>Путь запроса без параметров</summary>
    public string Path { get; set; } = null!;

    /// <summary>Полный путь запроса включая параметры строки запроса</summary>
    public string RequestPath => string.IsNullOrEmpty(_QueryString) ? Path : $"{Path}?{_QueryString}";

    /// <summary>Полный путь запроса с хостом</summary>
    public string FullRequestPath => $"{Host}{RequestPath}";

    /// <summary>Хост запроса из заголовка Host</summary>
    public string Host => GetHeader();

    /// <summary>Строка параметров запроса (всё после ?)</summary>
    /// <remarks>При установке автоматически разбирает параметры на пары ключ-значение</remarks>
    public string? QueryString
    {
        get => _QueryString;
        set
        {
            if (Equals(_QueryString, value)) return;
            _QueryString = value;
            _QueryParameters = _QueryString?
               .Split('&')
               .Select(v => v.Split('='))
               .Where(v => v.Length == 2)
               .Select(v => (v[0], v[1]))
               .ToArray()!;
        }
    }

    /// <summary>Параметры запроса в виде массива пар ключ-значение</summary>
    /// <remarks>При установке автоматически собирает строку запроса из параметров</remarks>
    public (string Key, string Value)[]? QueryParameters
    {
        get => _QueryParameters;
        set
        {
            if (ReferenceEquals(_QueryParameters, value)) return;
            _QueryParameters = value;
            _QueryString = value is null
                ? null
                : string.Join("&", value.Select(v => $"{v.Key}={v.Value}"));
        }
    }

    /// <summary>User-Agent клиента из заголовка</summary>
    public string UserAgentStr => GetHeader("User-Agent");

    /// <summary>Тип соединения из заголовка Connection</summary>
    public string Connection => GetHeader();

    /// <summary>Типы содержимого, которые клиент может принять</summary>
    public string Accept => GetHeader();

    /// <summary>Ссылка, с которой клиент пришёл на эту страницу</summary>
    /// <returns>URI реферера или null, если заголовок отсутствует</returns>
    public Uri? Referer
    {
        get
        {
            var referer = GetHeader();
            return string.IsNullOrWhiteSpace(referer) ? null : new Uri(referer);
        }
    }

    /// <summary>Методы кодирования контента, поддерживаемые клиентом</summary>
    public string AcceptEncoding => GetHeader("Accept-Encoding");

    /// <summary>Предпочтительные языки клиента</summary>
    public string AcceptLanguage => GetHeader("Accept-Language");

    /// <summary>Загружает HTTP-запрос из потока</summary>
    /// <param name="Reader">Потоковый считыватель для чтения данных запроса</param>
    /// <exception cref="FormatException">Если первая строка запроса имеет неверный формат или количество параметров</exception>
    public override void Load(StreamReader Reader)
    {
        base.Load(Reader);

        if (Reader.EndOfStream) throw new FormatException("Ошибка в первой строке");
        var line = Reader.ReadLine();
        if (string.IsNullOrWhiteSpace(line))
            throw new FormatException("Ошибка в первой строке");

        var components = line.Split(' ');
        if (components.Length < 3) throw new FormatException("Число параметров первой строки меньше 3");

        Method = components[0].ToUpper();
        var path_str = components[1];
        var path_str_components = path_str.Split('?');
        Path = path_str_components[0];
        QueryString = path_str_components.Length > 1 ? path_str_components[1] : null;
        Version = components[2];

        LoadHeaders(Reader);

        LoadContent(Reader.BaseStream);
    }
}