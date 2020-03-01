using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MathCore.NET.TCP;
using MathCore.NET.TCP.Events;

namespace MathCore.NET.TestConsole
{
    internal static class ServerHost
    {
        public static void Start(int Port)
        {
            var server_thread = new Thread(() => ServerThreadEntryPoint(Port)) { IsBackground = true };
            server_thread.Start();
        }

        private static void ServerThreadEntryPoint(int Port)
        {
            const string response_string = "Hello World!";

            var server = new Server(Port);
            server.ClientConnected += OnServerOnClientConnected;
            server.ClientDisconnected += OnServerOnClientDisconnected;
            server.DataReceived += (s, e) =>
            {
                Console.WriteLine("From client >> {0}({1}): {2}", e.Client.Host, e.Client.Port, e.ClientData.Message);

                e.Client.Send(
@$"HTTP/1.1 200 OK
Date: {DateTime.Now:F} GMT
Server: SimpleTCP
X-Powered-By: Infarh
Last-Modified: {DateTime.Now:F} GMT
Content-Language: ru
Content-Type: text/html; charset=utf-8
Content-Length: {response_string.Length}
Connection: close

{response_string}");
            };
            server.Start();
        }

        private static void OnServerOnClientDisconnected(object s, ClientEventArgs e)
        {
            Console.WriteLine("Client disconnected: {0}:{1}", e.Client.Host, e.Client.Port);
        }

        private static void OnServerOnClientConnected(object s, ClientEventArgs e)
        {
            Console.WriteLine("Client connected: {0}:{1}", e.Client.Host, e.Client.Port);
        }
    }
}
