using System;
using System.IO;

namespace MathCore.NET.HTTP
{
    public class Response : Message
    {
        public int Code { get; set; }

        public string Status { get; set; }

        public override void Load(StreamReader Reader)
        {
            base.Load(Reader);

            if (Reader.EndOfStream) throw new FormatException("Ошибка в первой строке");
            var line = Reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                throw new FormatException("Ошибка в первой строке");

            var components = line.Split(' ');
            if (components.Length < 3) throw new FormatException("Число параметров первой строки меньше 3");

            Version = components[0];
            Code = int.Parse(components[1]);
            Status = components[2];

            LoadHeaders(Reader);

            LoadContent(Reader.BaseStream);
        }
    }
}