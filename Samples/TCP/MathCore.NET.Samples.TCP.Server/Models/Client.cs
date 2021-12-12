using System;
using System.IO;

using MathCore.NET.TCP.Events;

namespace MathCore.NET.Samples.TCP.Server.Models
{
    class Client
    {
        private NET.TCP.Client _Client;

        public Client(NET.TCP.Client Client) => SubscribeToEvents(_Client = Client);

        private void SubscribeToEvents(NET.TCP.Client client)
        {
            client.DataReceived += OnDataReceived;
            client.DataSent += OnDataSend;
            client.Error += OnError;
            client.Disconnected += OnDisconnected;
        }

        private void UnsubscribeFromEvents(NET.TCP.Client client)
        {
            client.DataReceived -= OnDataReceived;
            client.DataSent -= OnDataSend;
            client.Error -= OnError;
            client.Disconnected -= OnDisconnected;
        }

        private void OnDisconnected(object? Sender, EventArgs E) => UnsubscribeFromEvents((NET.TCP.Client)Sender);

        private void OnError(object? Sender, ErrorEventArgs E)
        {

        }

        private void OnDataSend(object? Sender, DataEventArgs E)
        {

        }

        private void OnDataReceived(object? Sender, DataEventArgs E)
        {

        }
    }
}
