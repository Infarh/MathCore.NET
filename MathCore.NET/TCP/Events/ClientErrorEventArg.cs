using System;
using System.IO;

namespace MathCore.NET.TCP.Events;

public class ClientErrorEventArgs(Client Client, Exception exception) : ErrorEventArgs(exception)
{
    public Client Client { get; } = Client;
}