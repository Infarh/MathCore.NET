namespace MathCore.NET.TCP.Events
{
    public class ClientDataEventArgs : ClientEventArgs
    {
        public DataEventArgs ClientData { get; }

        public ClientDataEventArgs(Client Client, DataEventArgs Data) : base(Client) => ClientData = Data;
    }
}