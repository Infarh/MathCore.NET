using System;

namespace MathCore.NET.TestConsole;

internal class Program
{
    private const int __ServerPort = 8080;

    private static void Main(string[] args)
    {   
            ServerHost.Start(__ServerPort);
            Console.WriteLine("Server started...");
            Console.ReadLine();

            Console.WriteLine("End of process...");
            Console.ReadLine();
        }
}