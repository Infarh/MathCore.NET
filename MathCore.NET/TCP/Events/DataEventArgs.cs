using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Global

namespace MathCore.NET.TCP.Events
{
    /// <summary>Параметры передачи данных</summary>
    /// <remarks>Содержит данные в виде массива символов.</remarks>
    public class DataEventArgs : EventArgs
    {
        private readonly IFormatter _DataFormatter;
        private readonly byte[] _Data;
        private readonly int _ReadedDataLength;
        private readonly Encoding _DataEncoding;

        /// <summary>Данные в виде массива байт</summary>
        public IReadOnlyList<byte> Data => _Data;

        public int ReadedDataLength => _ReadedDataLength;

        public Encoding DataEncoding => _DataEncoding;

        /// <summary>
        /// Свойство, возвращающее массив строк, генерируемый разделением 
        /// исходных данные, используя в качестве разделителя символ конца строки
        /// </summary>
        public string Message => DataEncoding.GetString(_Data, 0, ReadedDataLength);

        public Stream DataStream => new MemoryStream(_Data, 0, ReadedDataLength);

        /// <summary>Конструктор из символьного массива</summary>
        /// <param name="Data">Данные</param>
        /// <param name="DataEncoding">Кодировка текста</param>
        /// <param name="DataFormatter">Объект десериализации</param>
        public DataEventArgs(byte[] Data, Encoding DataEncoding, IFormatter DataFormatter)
            : this(Data, Data.Length, DataEncoding, DataFormatter) { }

        /// <summary>Конструктор из символьного массива</summary>
        /// <param name="Data">Данные</param>
        /// <param name="ReadedDataLength">Число прочитанных байт</param>
        /// <param name="DataEncoding">Кодировка текста</param>
        /// <param name="DataFormatter">Объект десериализации</param>
        public DataEventArgs(byte[] Data, int ReadedDataLength, Encoding DataEncoding, IFormatter DataFormatter)
        {
            _Data = Data;
            _ReadedDataLength = ReadedDataLength;
            _DataEncoding = DataEncoding;
            _DataFormatter = DataFormatter;
        }

        protected object Deserialize()
        {
            using var stream = DataStream;
            return _DataFormatter.Deserialize(stream);
        }

        public T ReadAs<T>() => (T)Deserialize();

        /// <inheritdoc />
        public override string ToString() => Message;

        public static implicit operator byte[](DataEventArgs E) => E._Data;

        public static implicit operator string(DataEventArgs E) => E.Message;
    }
}