using System;
using System.Collections.Generic;

namespace MathCore.NET.Samples.TCP.Server.Services.Interfaces
{
    interface ITCPServer
    {
        event EventHandler<EventArgs<ITCPClient, string>> MessageReceived; 
        
        int Port { get; }

        bool Enabled { get; }

        ICollection<ITCPClient> Clients { get; }

        void Start(int port = 8080);

        void Stop();

        void SendMessage(string Message);
    }
}
