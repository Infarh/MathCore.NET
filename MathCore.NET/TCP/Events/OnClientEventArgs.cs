using System;

namespace MathCore.NET.TCP.Events;

/// <summary>Параметры события работы с клиентом</summary>
/// <remarks>Инициализация нового экземпляра <see cref="ClientEventArgs"/></remarks>
/// <param name="Client">Клиент</param>
public class ClientEventArgs(Client Client) : EventArgs
{
    /// <summary>Клиент</summary>
    public Client Client { get; } = Client;
}