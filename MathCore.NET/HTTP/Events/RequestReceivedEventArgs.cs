using System.Net;

namespace MathCore.NET.HTTP.Events;

public class RequestReceivedEventArgs(HttpListenerContext Context)
{
    public HttpListenerContext Context { get; } = Context;
}