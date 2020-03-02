using System;
using System.Net;

namespace MathCore.NET.UDP.Events
{
    /// <summary>Аргументы события получения данных</summary>
    public sealed class DataReceivedEventArgs : EventArgs
    {
        /// <summary>Полученные данные</summary>
        public readonly byte[] Data;
        /// <summary>Точка сети - источник данных</summary>
        public readonly IPEndPoint EndPoint;
        /// <summary>Новый аргумент получения данных</summary>
        /// <param name="Data">Полученные данные</param>
        /// <param name="EndPoint">Точка сети - источник данных</param>
        public DataReceivedEventArgs(byte[] Data, IPEndPoint EndPoint)
        {
            this.Data = Data;
            this.EndPoint = EndPoint;
        }
        /// <summary>Оператор неявного приведения типов к типу данных "массив байт"</summary>
        /// <param name="Arg">Аргумент события получения данных</param>
        /// <returns>Массив байт</returns>
        public static implicit operator byte[](DataReceivedEventArgs Arg) => Arg.Data;

        /// <summary>Оператор неявного приведения типов к типу данных адреса сети</summary>
        /// <param name="Arg">Аргумент события получения данных из сети</param>
        /// <returns>Точка в сети</returns>
        public static implicit operator IPEndPoint(DataReceivedEventArgs Arg) => Arg.EndPoint;
    }
}
