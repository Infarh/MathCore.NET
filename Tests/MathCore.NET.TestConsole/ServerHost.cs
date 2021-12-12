using System;
using System.Threading;
using MathCore.NET.HTTP;
using MathCore.NET.HTTP.Html;
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
            var server = new TCP.Server(Port);
            server.ClientConnected += OnServerOnClientConnected;
            server.ClientDisconnected += OnServerOnClientDisconnected;
            server.DataReceived += OnServerDataReceived;
            server.Start();
        }

        private static void OnServerDataReceived(object sender, ClientDataEventArgs e)
        {
            Console.WriteLine(new string('-', Console.BufferWidth));
            Console.WriteLine("From client >> {0}({1}): {2}", e.Client.Host, e.Client.Port, e.ClientData.Message);
            Console.WriteLine(new string('=', Console.BufferWidth));

            var request = new Request();
            request.Load(e.ClientData.DataStream);

            var page = new Page
            {
                Title = "Test!!!",
                Body = new Body
                {
                    new P(request.FullRequestPath)
                }
            };

            var response = page.ToString();

            e.Client.Send(@$"HTTP/1.1 200 OK
Date: {DateTime.Now:F} GMT
Server: SimpleTCP
X-Powered-By: Infarh
Last-Modified: {DateTime.Now:F} GMT
Content-Language: ru
Content-Type: text/html; charset=utf-8
Content-Length: {response.Length}
Connection: close

{response}");
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
