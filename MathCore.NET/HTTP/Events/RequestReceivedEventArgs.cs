using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MathCore.NET.HTTP.Events
{
    public class RequestReceivedEventArgs
    {
        public HttpListenerContext Context { get; }

        public RequestReceivedEventArgs(HttpListenerContext Context) => this.Context = Context;
    }
}
