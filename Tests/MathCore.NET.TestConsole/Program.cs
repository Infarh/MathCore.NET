using System;
using MathCore.NET.TCP;

namespace MathCore.NET.TestConsole
{
    class Program
    {
        private const int __ServerPort = 8080;

        static void Main(string[] args)
        {
            var user = Environment.UserName;

            ServerHost.Start(__ServerPort);
            Console.WriteLine("Server started...");
            Console.ReadLine();

            var client = new Client("127.0.0.1:8080");
            client.DataReceived += (s, e) => Console.WriteLine("From server >> {0}", e.Message);
            client.Start();
            Console.WriteLine("Client started...");
            Console.ReadLine();

            client.Send("Message from client!!!");
            Console.ReadLine();
            client.Stop();

            Console.WriteLine("End of process...");
            Console.ReadLine();
        }

    }
}
