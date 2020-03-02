using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MathCore.NET.TCP.Events
{
    public class ClientErrorEventArgs : ErrorEventArgs
    {
        public Client Client { get; }

        public ClientErrorEventArgs(Client Client, Exception exception) : base(exception) => this.Client = Client;
    }
}
