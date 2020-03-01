using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global

namespace MathCore.NET.TCP.Events
{
    /// <summary>Параметры передачи данных</summary>
    /// <remarks>Содержит данные в виде массива символов.</remarks>
    public class DataEventArgs : EventArgs
    {
        protected readonly IFormatter _DataFormatter;

        /// <summary>Данные в виде массива байт</summary>
        public byte[] Data { get; }

        public Encoding DataEncoding { get; }

        /// <summary>
        /// Свойство, возвращающее массив строк, генерируемый разделением 
        /// исходных данные, используя в качестве разделителя символ конца строки
        /// </summary>
        public string Message => DataEncoding.GetString(Data);

        public Stream DataStream => new MemoryStream(Data);

        public object Object => Deserialize();

        /// <summary>Конструктор из символьного массива</summary>
        /// <param name="Data">Данные</param>
        /// <param name="DataEncoding">Кодировка текста</param>
        /// <param name="DataFormatter">Объект десериализации</param>
        public DataEventArgs(byte[] Data, Encoding DataEncoding, IFormatter DataFormatter)
        {
            this.Data = Data;
            this.DataEncoding = DataEncoding;
            this._DataFormatter = DataFormatter;
        }

        protected object Deserialize()
        {
            using var stream = DataStream;
            return _DataFormatter.Deserialize(stream);
        }

        /// <inheritdoc />
        public override string ToString() => Message;
    }
}