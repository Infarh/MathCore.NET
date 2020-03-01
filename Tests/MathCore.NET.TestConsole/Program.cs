using System;

namespace MathCore.NET.TestConsole
{
    class Program
    {
        private const int __ServerPort = 8080;

        static void Main(string[] args)
        {
            ServerHost.Start(__ServerPort);
            

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

    }
}
