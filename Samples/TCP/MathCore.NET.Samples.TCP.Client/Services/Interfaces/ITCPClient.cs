using System;

namespace MathCore.NET.Samples.TCP.Client.Services.Interfaces
{
    interface ITCPClient
    {
        event EventHandler<EventArgs<string>> ReceiveMessage;
        
        public string Address { get; }

        public int Port { get; }

        public bool Connected { get; }

        public void Connect(string address, int Port = 80);

        public void Disconnect();

        public void SendMessage(string Message);
    }
}
