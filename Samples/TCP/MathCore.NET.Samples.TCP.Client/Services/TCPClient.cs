using System;
using MathCore.NET.Samples.TCP.Client.Services.Interfaces;
using MathCore.NET.TCP.Events;

namespace MathCore.NET.Samples.TCP.Client.Services
{
    class TCPClient : ITCPClient, IDisposable
    {
        private NET.TCP.Client _Client;

        public event EventHandler<EventArgs<string>> ReceiveMessage;

        public bool Connected => _Client != null;

        public string Address => _Client?.Host;

        public int Port => _Client?.Port ?? -1;

        public void Connect(string address, int port)
        {
            if(Connected) 
                throw new InvalidOperationException("Клиент уже подключён");

            _Client = new NET.TCP.Client(address, port);
            _Client.DataReceived += OnDataReceived;
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
                _Client.Dispose();
            }
            finally
            {
                _Client = null;
            }
        }

        public void SendMessage(string Message) => _Client.Send(Message);

        private void CheckConnection()
        {
            if (!Connected)
                throw new InvalidOperationException("Подключение отсутствует");
        }

        private void OnDataReceived(object? Sender, DataEventArgs E) => ReceiveMessage?.Invoke(this, new EventArgs<string>(E.Message));

        public void Dispose() => _Client?.Dispose();
    }
}
