using System.Net;
using System.Net.Sockets;

namespace MathCore.NET.UDP.Service
{
    internal static class UdpReceiveResultExtensions
    {
        public static void Deconstruct(this UdpReceiveResult result, out byte[] Buffer, out IPEndPoint RemoteAddress)
        {
            Buffer = result.Buffer;
            RemoteAddress = result.RemoteEndPoint;
        }
    }
}
