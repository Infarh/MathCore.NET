using System;
using System.IO;
using System.Runtime.Serialization;
using MathCore.NET.TCP;

namespace MathCore.NET.Examples;

/// <summary>Пример использования JsonFormatter</summary>
public class JsonFormatterExample
{
    public class Message
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public int Id { get; set; }
    }

    public static void Main()
    {
        var formatter = new JsonFormatter();

        // Пример 1: Сериализация простой строки
        Console.WriteLine("=== Пример 1: Сериализация строки ===");
        SerializeAndDeserialize(formatter, "Hello, TCP World!");

        // Пример 2: Сериализация числа
        Console.WriteLine("\n=== Пример 2: Сериализация числа ===");
        SerializeAndDeserialize(formatter, 42);

        // Пример 3: Сериализация DateTime
        Console.WriteLine("\n=== Пример 3: Сериализация DateTime ===");
        SerializeAndDeserialize(formatter, DateTime.Now);

        // Пример 4: Сериализация объекта
        Console.WriteLine("\n=== Пример 4: Сериализация объекта ===");
        var message = new Message
        {
            Id = 1,
            Text = "Sample message",
            Timestamp = DateTime.Now
        };
        SerializeAndDeserialize(formatter, message);
    }

    private static void SerializeAndDeserialize(IFormatter formatter, object obj)
    {
        using (var stream = new MemoryStream())
        {
            // Сериализация
            formatter.Serialize(stream, obj);
            var serializedSize = stream.Length;
            Console.WriteLine($"Объект: {obj}");
            Console.WriteLine($"Размер сериализованных данных: {serializedSize} байт");

            // Десериализация
            stream.Position = 0;
            var deserialized = formatter.Deserialize(stream);
            Console.WriteLine($"Десериализовано: {deserialized}");
        }
    }
}
