using System;

namespace MathCore.NET.TCP.Events
{
    /// <summary>Параметры события работы с клиентом</summary>
    public class ClientEventArgs : EventArgs
    {
        /// <summary>Клиент</summary>
        public Client Client { get; }

        /// <summary>Инициализация нового экземпляра <see cref="ClientEventArgs"/></summary>
        /// <param name="Client">Клиент</param>
        public ClientEventArgs(Client Client) => this.Client = Client;
    }
}
