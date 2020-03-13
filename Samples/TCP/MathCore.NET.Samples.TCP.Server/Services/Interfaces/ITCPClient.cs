using System;

namespace MathCore.NET.Samples.TCP.Server.Services.Interfaces
{
    interface ITCPClient
    {
        event EventHandler<EventArgs<string>> DataReceived;

        public string Host { get; }

        public int Port { get; }

        public void Send(string Message);
    }
}