using System;
using MathCore.NET.Samples.TCP.Client.Services.Interfaces;

namespace MathCore.NET.Samples.TCP.Client.Services
{
    class TCPClient : ITCPClient, IDisposable
    {
        private NET.TCP.Client _Client;

        public bool Connected => _Client != null;

        public string Address => _Client?.Host;

        public int Port => _Client?.Port ?? -1;

        public void Connect(string address, int port)
        {
            if(Connected) 
                throw new InvalidOperationException("Клиент уже подключён");

            _Client = new NET.TCP.Client(address, port);
            try
            {
                _Client.Start();
            }
            catch
            {
                _Client = null;
                throw;
            }
        }

        public void Disconnect()
        {
            CheckConnection();

            try
            {
                _Client.Stop();
                _Client.Dispose();
            }
            finally
            {
                _Client = null;
            }
        }

        private void CheckConnection()
        {
            if (!Connected)
                throw new InvalidOperationException("Подключение отсутствует");
        }

        public void Dispose() => _Client?.Dispose();
    }
}
