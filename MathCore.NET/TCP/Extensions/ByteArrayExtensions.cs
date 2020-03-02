namespace MathCore.NET.TCP.Extensions
{
    internal static class ByteArrayExtensions
    {
        public static ushort GetIPChecksum(this byte[] buffer, ushort checksum = 0, int offset = 0, int length = 0)
        {
            if (length == 0) length = buffer.Length - offset;

            var s = 0;
            for (var i = offset; i < length + offset; i += 2)
            {
                var v = (buffer[i] << 8) + buffer[i + 1];
                if (v == checksum) continue;
                s += v;
            }
            return (ushort)~(s + (s >> 16));
        }
    }
}
