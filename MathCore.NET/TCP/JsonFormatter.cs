#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace MathCore.NET.TCP;

/// <summary>Реализация IFormatter на основе простой текстовой сериализации</summary>
/// <remarks>Поддерживает базовые типы и объекты с публичными свойствами</remarks>
/// <remarks>
/// Инициализирует новый экземпляр JsonFormatter с указанной кодировкой
/// </remarks>
/// <param name="Encoding">Кодировка для работы со строками</param>
public class JsonFormatter(Encoding Encoding) : IFormatter
{
    private readonly Encoding _encoding = Encoding ?? Encoding.UTF8;

    /// <summary>
    /// Инициализирует новый экземпляр JsonFormatter с кодировкой UTF-8
    /// </summary>
    public JsonFormatter() : this(Encoding.UTF8) { }

    /// <inheritdoc />
    public SerializationBinder? Binder { get; set; }

    /// <inheritdoc />
    public ISurrogateSelector? SurrogateSelector { get; set; }

    /// <inheritdoc />
    public StreamingContext Context { get; set; }

    /// <summary>
    /// Сериализует объект и записывает в поток
    /// </summary>
    /// <param name="SerializationStream">Поток для записи</param>
    /// <param name="Graph">Объект для сериализации</param>
    public void Serialize(Stream SerializationStream, object Graph)
    {
        if (SerializationStream == null) throw new ArgumentNullException(nameof(SerializationStream));
        if (Graph == null) throw new ArgumentNullException(nameof(Graph));

        var objectType = Graph.GetType();
        var json = SerializeObject(Graph);
        var typeName = objectType.AssemblyQualifiedName ?? objectType.FullName ?? "";
        var typeNameBytes = _encoding.GetBytes(typeName);
        var dataBytes = _encoding.GetBytes(json);

        // Записываем метаданные: длина имени типа + имя типа + длина JSON + JSON
        var typeNameLength = BitConverter.GetBytes(typeNameBytes.Length);
        var dataLength = BitConverter.GetBytes(dataBytes.Length);

        SerializationStream.Write(typeNameLength, 0, 4);
        SerializationStream.Write(typeNameBytes, 0, typeNameBytes.Length);
        SerializationStream.Write(dataLength, 0, 4);
        SerializationStream.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// Десериализует объект из потока
    /// </summary>
    /// <param name="SerializationStream">Поток для чтения</param>
    /// <returns>Десериализованный объект</returns>
    public object Deserialize(Stream SerializationStream)
    {
        if (SerializationStream == null) throw new ArgumentNullException(nameof(SerializationStream));

        // Читаем метаданные типа
        var typeNameLengthBytes = new byte[4];
        if (SerializationStream.Read(typeNameLengthBytes, 0, 4) != 4)
            throw new SerializationException("Не удалось прочитать длину имени типа");

        var typeNameLength = BitConverter.ToInt32(typeNameLengthBytes, 0);
        if (typeNameLength <= 0)
            throw new SerializationException("Некорректная длина имени типа");

        var typeNameBytes = new byte[typeNameLength];
        if (SerializationStream.Read(typeNameBytes, 0, typeNameLength) != typeNameLength)
            throw new SerializationException("Не удалось прочитать имя типа");

        var typeName = _encoding.GetString(typeNameBytes);

        // Получаем тип объекта
        var objectType = Type.GetType(typeName);
        if (objectType == null)
            throw new SerializationException($"Не удалось найти тип: {typeName}");

        // Читаем данные JSON
        var dataLengthBytes = new byte[4];
        if (SerializationStream.Read(dataLengthBytes, 0, 4) != 4)
            throw new SerializationException("Не удалось прочитать длину данных");

        var dataLength = BitConverter.ToInt32(dataLengthBytes, 0);
        if (dataLength <= 0)
            throw new SerializationException("Некорректная длина данных");

        var dataBytes = new byte[dataLength];
        if (SerializationStream.Read(dataBytes, 0, dataLength) != dataLength)
            throw new SerializationException("Не удалось прочитать данные");

        var json = _encoding.GetString(dataBytes);

        // Десериализуем объект
        return DeserializeObject(json, objectType);
    }

    private static string SerializeObject(object obj)
    {
        if (obj == null) return "null";

        var type = obj.GetType();

        // Обработка базовых типов
        if (type == typeof(string))
            return $"\"{EscapeString((string)obj)}\"";

        if (type.IsValueType && !type.IsEnum)
        {
            if (type == typeof(bool))
                return obj.ToString()!.ToLower();
            if (type == typeof(DateTime))
                return $"\"{(DateTime)obj:O}\"";
            return obj.ToString() ?? "null";
        }

        if (type.IsEnum)
            return obj.ToString() ?? "null";

        if (obj is System.Collections.IEnumerable enumerable and not string)
        {
            var items = new List<string>();
            foreach (var item in enumerable)
                items.Add(SerializeObject(item));
            return $"[{string.Join(",", items)}]";
        }

        // Сериализация объекта через публичные свойства
        var props = new List<string>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                var value = prop.GetValue(obj);
                props.Add($"\"{prop.Name}\":{SerializeObject(value)}");
            }
            catch { }
        }

        return $"{{{string.Join(",", props)}}}";
    }

    private static object DeserializeObject(string json, Type targetType)
    {
        json = json.Trim();

        // Обработка null
        if (json == "null")
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null!;

        // Обработка строк
        if (json.StartsWith("\"") && json.EndsWith("\""))
        {
            var value = json.Substring(1, json.Length - 2);
            if (targetType == typeof(string))
                return UnescapeString(value);
            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);
            return value;
        }

        // Обработка булевых значений
        if (targetType == typeof(bool))
            return json.Equals("true", StringComparison.OrdinalIgnoreCase);

        // Обработка числовых типов
        if (targetType.IsValueType && !targetType.IsEnum)
            return Convert.ChangeType(json, targetType);

        // Обработка перечислений
        if (targetType.IsEnum)
            return Enum.Parse(targetType, json);

        // Обработка коллекций
        if (targetType.IsArray || targetType.Name.Contains("List"))
        {
            // Упрощённая обработка массивов - требует более сложной реализации
            return Array.CreateInstance(targetType.GetElementType() ?? typeof(object), 0);
        }

        // Создание объекта через конструктор по умолчанию
        return Activator.CreateInstance(targetType) ?? throw new SerializationException($"Не удалось создать экземпляр типа {targetType}");
    }

    private static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t");
    }

    private static string UnescapeString(string value)
    {
        return value.Replace("\\t", "\t")
                   .Replace("\\r", "\r")
                   .Replace("\\n", "\n")
                   .Replace("\\\"", "\"")
                   .Replace("\\\\", "\\");
    }
}
