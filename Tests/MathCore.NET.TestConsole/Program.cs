using MathCore.NET.TestConsole;
using System;

const int server_port = 8080;

ServerHost.Start(server_port);
Console.WriteLine("Server started...");
Console.ReadLine();

Console.WriteLine("End of process...");
Console.ReadLine();

return;