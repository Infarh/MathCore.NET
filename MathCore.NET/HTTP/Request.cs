using System;
using System.IO;

namespace MathCore.NET.HTTP
{
    public class Request : Message
    {
        public string Method { get; set; }

        public string Path { get; set; }

        public string Host => GetHeader();
        
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
            Path = components[1];
            Version = components[2];

            LoadHeaders(Reader);

            LoadContent(Reader.BaseStream);
        }
    }
}