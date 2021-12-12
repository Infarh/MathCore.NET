using System;
using System.IO;
using System.Linq;

namespace MathCore.NET.HTTP
{
    public class Request : Message
    {
        private string _QueryString;
        private (string Key, string Value)[] _QueryParameters;

        public string Method { get; set; }

        public string Path { get; set; }

        public string RequestPath => string.IsNullOrEmpty(_QueryString) ? Path : $"{Path}?{_QueryString}";

        public string FullRequestPath => $"{Host}{RequestPath}";

        public string Host => GetHeader();

        public string QueryString
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
                   .ToArray();
            }
        }

        public (string Key, string Value)[] QueryParameters
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

        public string UserAgent => GetHeader("User-Agent");

        public string Connection => GetHeader();
        public string Accept => GetHeader();

        public Uri Referer
        {
            get
            {
                var referer = GetHeader();
                return string.IsNullOrWhiteSpace(referer) ? null : new Uri(referer);
            }
        }

        public string AcceptEncoding => GetHeader("Accept-Encoding");

        public string AcceptLanguage => GetHeader("Accept-Language");

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
}