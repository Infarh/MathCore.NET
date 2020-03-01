using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Net.Sockets
{
    internal static class SocketExtensions
    {
        public static Task DisconnectAsync(this Socket socket)
        {
            static IAsyncResult BeginMethod(AsyncCallback callback, object obj) => ((Socket)obj).BeginDisconnect(false, callback, obj);
            static void EndMethod(IAsyncResult result) => ((Socket)result.AsyncState).EndAccept(result);
            return Task.Factory.FromAsync(BeginMethod, EndMethod, socket);
        }
    }
}
