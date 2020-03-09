using System;
using MathCore.NET.Samples.TCP.Server.Services.Interfaces;

namespace MathCore.NET.Samples.TCP.Server.Services
{
    class TCPServer : ITCPServer
    {
        private NET.TCP.Server _Server;

        public int Port => _Server?.Port ?? -1;

        public bool Enabled => _Server?.Enabled ?? false;

        public void Start(int port = 8080)
        {
            if (Enabled)
                throw new InvalidOperationException("Сервер активен");

            try
            {
                _Server = new NET.TCP.Server(port);
                _Server.Start();
            }
            catch
            {
                _Server?.Dispose();
                _Server = null;
                throw;
            }
        }

        public void Stop()
        {
            CheckActivity();
            try
            {
                _Server.Stop();
                _Server.Dispose();
            }
            finally
            {
                _Server = null;
            }
        }

        private void CheckActivity()
        {
            if (!Enabled)
                throw new InvalidOperationException("Сервер не активен");
        }
    }
}
