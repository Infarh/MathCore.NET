using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.NET.TCP.Events;

namespace MathCore.NET.Samples.TCP.Server.Services
{
    class TCPServer : ITCPServer
    {
        private NET.TCP.Server _Server;
        private readonly ObservableCollection<ITCPClient> _Clients = new ObservableCollection<ITCPClient>();

        public int Port => _Server?.Port ?? -1;

        public bool Enabled => _Server?.Enabled ?? false;

        public ICollection<ITCPClient> Clients => _Clients;

        public void Start(int port = 8080)
        {
            if (Enabled)
                throw new InvalidOperationException("Сервер активен");

            NET.TCP.Server server = null;
            try
            {
                server = new NET.TCP.Server(port);
                InitializeServer(server);
                server.Start();
                _Server = server;
            }
            catch
            {
                server?.Dispose();
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

        private void InitializeServer(NET.TCP.Server Server)
        {
            Server.ClientConnected += OnClientConnected;
            Server.ClientDisconnected += OnClientDisconnected;
        }

        private void OnClientConnected(object? Sender, ClientEventArgs E) => _Clients.Add(new TCPClient(E.Client));

        private void OnClientDisconnected(object? Sender, ClientEventArgs E)
        {
            var client_to_remove = _Clients.OfType<TCPClient>().FirstOrDefault(c => ReferenceEquals(c.Client, E.Client));
            if (client_to_remove is null) return;
            _Clients.Remove(client_to_remove);
        }
    }
}
