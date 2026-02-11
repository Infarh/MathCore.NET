using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MathCore.NET.HTTP;

/// <summary>Базовый класс для HTTP-сообщений (запроса и ответа) с поддержкой заголовков и содержимого</summary>
public abstract class Message : IEnumerable<(string Header, string Value)>
{
    //private byte[] _Content;

    protected readonly List<(string Header, string Value)> _Headers = [];

    /// <summary>User-Agent клиента из заголовка</summary>
    public string UserAgent => GetHeader("User-Agent");

    /// <summary>Куки из заголовка Cookie</summary>
    public string Cookie => GetHeader();

    /// <summary>Версия протокола HTTP</summary>
    public string Version { get; set; }

    /// <summary>Получает все значения заголовка по его имени</summary>
    /// <param name="HeaderName">Имя заголовка для поиска</param>
    /// <returns>Перечисление значений заголовков с указанным именем</returns>
    public IEnumerable<string> this[string HeaderName] => GetHeaders(HeaderName);

    /// <summary>Загружает сообщение из потока</summary>
    /// <param name="DataStream">Поток с данными сообщения</param>
    /// <exception cref="ArgumentNullException">Если DataStream равен null</exception>
    public virtual void Load(Stream DataStream) => Load(new StreamReader(DataStream ?? throw new ArgumentNullException(nameof(DataStream))));

    /// <summary>Асинхронно загружает сообщение из потока</summary>
    /// <param name="DataStream">Поток с данными сообщения</param>
    /// <param name="Cancel">Токен отмены операции</param>
    /// <exception cref="ArgumentNullException">Если DataStream равен null</exception>
    /// <returns>Задача загрузки сообщения</returns>
    public virtual async Task LoadAsync(Stream DataStream, CancellationToken Cancel = default) => await LoadAsync(new StreamReader(DataStream ?? throw new ArgumentNullException(nameof(DataStream))), Cancel).ConfigureAwait(false);

    /// <summary>Загружает сообщение из потокового считывателя</summary>
    /// <param name="Reader">Считыватель для чтения данных сообщения</param>
    /// <exception cref="ArgumentNullException">Если Reader равен null</exception>
    public virtual void Load(StreamReader Reader)
    {
        if (Reader is null) throw new ArgumentNullException(nameof(Reader));
    }

    /// <summary>Асинхронно загружает сообщение из потокового считывателя</summary>
    /// <param name="Reader">Считыватель для чтения данных сообщения</param>
    /// <param name="Cancel">Токен отмены операции</param>
    /// <exception cref="ArgumentNullException">Если Reader равен null</exception>
    /// <returns>Завершённая задача загрузки</returns>
    public virtual Task LoadAsync(StreamReader Reader, CancellationToken Cancel = default) =>
        Reader is null ? throw new ArgumentNullException(nameof(Reader)) : Task.CompletedTask;

    /// <summary>Получает значение заголовка по его имени (без учёта регистра)</summary>
    /// <param name="HeaderName">Имя заголовка для поиска (автоматически берётся из имени вызывающего свойства)</param>
    /// <returns>Значение заголовка или null, если заголовок не найден</returns>
    public string? GetHeader([CallerMemberName] string? HeaderName = null)
    {
        foreach (var (key, value) in _Headers)
            if (key.Equals(HeaderName, StringComparison.OrdinalIgnoreCase))
                return value;
        return null;
    }

    /// <summary>Получает все значения заголовка по его имени (без учёта регистра)</summary>
    /// <param name="HeaderName">Имя заголовка для поиска (автоматически берётся из имени вызывающего свойства)</param>
    /// <returns>Перечисление всех значений заголовков с указанным именем</returns>
    public IEnumerable<string> GetHeaders([CallerMemberName] string? HeaderName = null)
    {
        foreach (var (key, value) in _Headers)
            if (key.Equals(HeaderName, StringComparison.OrdinalIgnoreCase))
                yield return value;
    }

    /// <summary>Загружает заголовки сообщения из потокового считывателя</summary>
    /// <param name="Reader">Считыватель для чтения заголовков</param>
    protected void LoadHeaders(StreamReader Reader)
    {
        _Headers.Clear();
        string line;
        while (!Reader.EndOfStream && !string.IsNullOrWhiteSpace(line = Reader.ReadLine()))
            _Headers.Add(Parse(line));
        _Headers.TrimExcess();
    }

    /// <summary>Асинхронно загружает заголовки сообщения из потокового считывателя</summary>
    /// <param name="Reader">Считыватель для чтения заголовков</param>
    /// <param name="Cancel">Токен отмены операции</param>
    /// <returns>Задача загрузки заголовков</returns>
    protected async Task LoadHeadersAsync(StreamReader Reader, CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        _Headers.Clear();
        string line;
        while (!Reader.EndOfStream && !string.IsNullOrWhiteSpace(line = await Reader.ReadLineAsync().WithCancellation(Cancel).ConfigureAwait(false)))
        {
            Cancel.ThrowIfCancellationRequested();
            _Headers.Add(Parse(line));
        }
        _Headers.TrimExcess();
    }

    /// <summary>Разбирает строку заголовка HTTP на имя и значение</summary>
    /// <param name="line">Строка заголовка в формате "Name: Value"</param>
    /// <returns>Кортеж с именем заголовка и его значением</returns>
    protected static (string Header, string Value) Parse(string line)
    {
        var separator_index = line.IndexOf(':');
        if (separator_index <= 0) return ("unknown", line);
        var header = line.Substring(0, separator_index);
        var value = line.Substring(separator_index + 2);
        return (header, value);
    }

    /// <summary>Загружает содержимое сообщения из потока</summary>
    /// <param name="DataStream">Поток с данными содержимого</param>
    protected void LoadContent(Stream DataStream)
    {
        //_Content = null;
        if (DataStream.Position == DataStream.Length) return;
        var content = new byte[DataStream.Length - DataStream.Position];
        DataStream.Read(content, 0, content.Length);
    }

    /// <summary>Асинхронно загружает содержимое сообщения из потока</summary>
    /// <param name="DataStream">Поток с данными содержимого</param>
    /// <param name="Cancel">Токен отмены операции</param>
    /// <returns>Задача загрузки содержимого</returns>
    protected async Task LoadContentAsync(Stream DataStream, CancellationToken Cancel = default)
    {
        //_Content = null;
        if (DataStream.Position == DataStream.Length) return;
        var content = new byte[DataStream.Length - DataStream.Position];
        await DataStream.ReadAsync(content, 0, content.Length, Cancel);
    }

    /// <summary>Получает перечислитель по заголовтам сообщения</summary>
    /// <returns>Перечислитель пар (имя заголовка, значение)</returns>
    public IEnumerator<(string Header, string Value)> GetEnumerator() => _Headers.GetEnumerator();

    /// <summary>Получает перечислитель по заголовтам сообщения</summary>
    /// <returns>Перечислитель пар (имя заголовка, значение)</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_Headers).GetEnumerator();
}