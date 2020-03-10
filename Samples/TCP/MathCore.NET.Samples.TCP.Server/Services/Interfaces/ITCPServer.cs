namespace MathCore.NET.Samples.TCP.Server.Services.Interfaces
{
    interface ITCPServer
    {
        int Port { get; }

        bool Enabled { get; }

        void Start(int port = 8080);

        void Stop();
    }
}
