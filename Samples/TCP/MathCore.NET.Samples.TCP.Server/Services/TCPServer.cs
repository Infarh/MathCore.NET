using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.NET.TCP;
using MathCore.NET.TCP.Events;

namespace MathCore.NET.Samples.TCP.Server.Services
{
    class TCPServer : ITCPServer
    {
        private NET.TCP.Server _Server;
        private readonly ConcurrentDictionary<Client, ITCPClient> _ClientsDictionary = new();
        private readonly ObservableCollection<ITCPClient> _Clients = new();

        public event EventHandler<EventArgs<ITCPClient, string>> MessageReceived;
        
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

        public void SendMessage(string Message)
        {
            foreach (var client in Clients)
                client.Send(Message);
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
            Server.DataReceived += OnDataReceived;
        }

        private void OnDataReceived(object? Sender, ClientDataEventArgs E)
        {
            var tcp_client = E.Client;
            var client = _Clients.FirstOrDefault(c => ((TCPClient)c).Client == tcp_client);
            if (client is null) return;
            var message = E.ClientData.Message;
            MessageReceived?.Invoke(this, new EventArgs<ITCPClient, string>(client, message));
        }

        private async void OnClientConnected(object Sender, ClientEventArgs E)
        {
            await Application.Current.Dispatcher;
            _Clients.Add(new TCPClient(E.Client));
        }

        private async void OnClientDisconnected(object Sender, ClientEventArgs E)
        {
            var client_to_remove = _Clients.OfType<TCPClient>().FirstOrDefault(c => ReferenceEquals(c.Client, E.Client));
            if (client_to_remove is null) return;
            await Application.Current.Dispatcher;
            _Clients.Remove(client_to_remove);
        }
    }
}
