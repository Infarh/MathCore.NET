using System.Net;

namespace MathCore.NET.HTTP.Events
{
    public class RequestReceivedEventArgs
    {
        public HttpListenerContext Context { get; }

        public RequestReceivedEventArgs(HttpListenerContext Context) => this.Context = Context;
    }
}
