using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

using MathCore.NET.HTTP.Html;
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.NET.HTTP;

/// <summary>Содержит информацию о входящем HTTP-запросе и предоставляет методы для работы с ответом</summary>
public class RequestInfo(HttpListenerContext context, Match match, WebServer Server) : EventArgs, IDisposable
{
    private readonly WebServer _Server = Server;

    /// <summary>Контекст HTTP-запроса от слушателя</summary>
    public HttpListenerContext Context { get; } = context;

    /// <summary>Результат совпадения маршрута с регулярным выражением</summary>
    public Match RouteRegexMatch { get; } = match;

    /// <summary>Сервер, обработавший запрос</summary>
    public WebServer Server => _Server;

    /// <summary>Бинарный считыватель данных запроса</summary>
    public BinaryReader BinaryReader => new(Context.Request.InputStream);

    /// <summary>Текстовый считыватель данных запроса</summary>
    public StreamReader Reader => new(Context.Request.InputStream);

    /// <summary>Бинарный писатель для ответа</summary>
    public BinaryWriter BinaryWriter => new(Context.Response.OutputStream);

    /// <summary>Текстовый писатель для ответа с автоматической очисткой буфера</summary>
    public StreamWriter Writer => new(Context.Response.OutputStream) { AutoFlush = true };

    /// <summary>URI запроса</summary>
    public Uri URI => Context.Request.Url;

    /// <summary>Тип содержимого ответа</summary>
    public string ContentType { get => Context.Response.ContentType; set => Context.Response.ContentType = value; }

    /// <summary>Оставить поток открытым после завершения обработки</summary>
    public bool LeaveOpen { get; set; }

    /// <summary>Устанавливает тип содержимого ответа</summary>
    /// <param name="type">MIME-тип содержимого</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    public RequestInfo SetContentType(string type)
    {
        ContentType = type;
        return this;
    }

    /// <summary>Устанавливает код статуса ответа</summary>
    /// <param name="StatusCode">Числовой код статуса HTTP</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    public RequestInfo SetStatusCode(int StatusCode)
    {
        Context.Response.StatusCode = StatusCode;
        return this;
    }

    /// <summary>Устанавливает код статуса ответа</summary>
    /// <param name="StatusCode">Код статуса HTTP</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    public RequestInfo SetStatusCode(HttpStatusCode StatusCode)
    {
        Context.Response.StatusCode = (int)StatusCode;
        return this;
    }

    /// <summary>Отправляет файл с указанным путём</summary>
    /// <param name="FileName">Путь к файлу для отправки</param>
    /// <param name="BufferLength">Размер буфера передачи в байтах</param>
    /// <param name="progress">Обработчик прогресса передачи файла (значение от 0 до 1)</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    /// <remarks>
    /// Если файл не найден в указанном пути, ищет его в корневой директории сервера.
    /// При ошибке устанавливает статус 500 Internal Server Error.
    /// </remarks>
    public RequestInfo SendFile(string FileName, int BufferLength = 1048, IProgress<double>? progress = null)
    {
        var response = Context.Response;
        var file = new FileInfo(FileName);

        if (!file.Exists)
            file = new(Path.Combine(_Server.HomeDirectoryPath, FileName));

        if (!file.Exists)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return this;
        }

        try
        {
            var response_stream = response.OutputStream;
            using var file_stream = file.OpenRead();
            response.ContentLength64 = file_stream.Length;
            var buffer = new byte[BufferLength];
            int readed;
            do
            {
                readed = file_stream.Read(buffer, 0, BufferLength);
                response_stream.Write(buffer, 0, readed);
                progress?.Report((double)file_stream.Position / file_stream.Length);
            } while (readed == BufferLength);
        }
        catch (Exception e)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Trace.TraceError(e.ToString());
            return this;
        }
        return this;
    }

    /// <summary>Отправляет файл из корневой директории сервера на основе пути запроса</summary>
    /// <param name="BufferLength">Размер буфера передачи в байтах</param>
    /// <param name="Progress">Обработчик прогресса передачи файла (значение от 0 до 1)</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    /// <remarks>
    /// Использует URI запроса для определения пути к файлу в корневой директории сервера.
    /// При ошибке устанавливает статус 500 Internal Server Error.
    /// </remarks>
    public RequestInfo SendFile(int BufferLength = 1048, IProgress<double>? Progress = null)
    {
        var file = new FileInfo(Path.Combine(_Server.HomeDirectoryPath, URI.LocalPath.TrimStart('/')));
        var response = Context.Response;
        if (!file.Exists)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return this;
        }

        try
        {
            var response_stream = response.OutputStream;
            using var file_stream = file.OpenRead();
            response.ContentLength64 = file_stream.Length;
            var buffer = new byte[BufferLength];
            int readed;
            do
            {
                readed = file_stream.Read(buffer, 0, BufferLength);
                response_stream.Write(buffer, 0, readed);
                Progress?.Report((double)file_stream.Position / file_stream.Length);
            } while (readed == BufferLength);
        }
        catch (Exception e)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Trace.TraceError(e.ToString());
            return this;
        }

        return this;
    }

    /// <summary>Отправляет текстовую строку в ответе</summary>
    /// <param name="text">Текст для отправки</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    public RequestInfo Send(string text)
    {
        Writer.Write(text);
        return this;
    }

    /// <summary>Отправляет HTML-страницу в ответе</summary>
    /// <param name="page">HTML-страница для отправки</param>
    /// <returns>Текущий экземпляр для цепочки вызовов</returns>
    public RequestInfo Send(Page page) => Send(page.ToString());

    /// <inheritdoc />
    public void Dispose()
    {
        if (!LeaveOpen) Context.Response.OutputStream.Dispose();
    }
}