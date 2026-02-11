namespace MathCore.NET.TCP.Events;

public class ClientDataEventArgs(Client Client, DataEventArgs Data) : ClientEventArgs(Client)
{
    public DataEventArgs ClientData { get; } = Data;
}