using System;
using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.NET.TCP;
using MathCore.NET.TCP.Events;

namespace MathCore.NET.Samples.TCP.Server.Services
{
    class TCPClient : ITCPClient
    {
        public event EventHandler<EventArgs<string>> DataReceived;

        protected virtual void OnDataReceived(EventArgs<string> E) => DataReceived?.Invoke(this, E);

        private readonly Client _Client;

        public Client Client => _Client;

        public string Host => _Client.Host;

        public int Port => _Client.Port;

        public TCPClient(Client Client) => Initialize(_Client = Client);

        private void Initialize(Client client) => client.DataReceived += OnDataReceived;

        public void Send(string Message) => _Client.Send(Message);

        private void OnDataReceived(object Sender, DataEventArgs E) => OnDataReceived(new EventArgs<string>(E.Message));
    }
}