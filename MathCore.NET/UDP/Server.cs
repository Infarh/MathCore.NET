using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MathCore.NET.UDP.Events;
using MathCore.NET.UDP.Service;

// ReSharper disable UnusedType.Global
// ReSharper disable EventNeverSubscribedTo.Global

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global

namespace MathCore.NET.UDP
{
    /// <summary>Объект прослушивания сетевого UDP порта</summary>
    public class Server : IDisposable
    {
        public event EventHandler Connected;

        protected virtual void OnConnected(EventArgs e) => Connected?.Invoke(this, e);

        public event EventHandler Disconnected;

        protected virtual void OnDisconnected(EventArgs e) => Disconnected?.Invoke(this, e);

        /// <summary>Событие получения данных из сети</summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>Генератор события получения данных из сети</summary>
        /// <param name="e">Аргумент события</param>
        protected virtual void OnDataReceived(DataReceivedEventArgs e) => DataReceived?.Invoke(this, e);

        /// <summary>Генератор события получения данных из сети</summary>
        /// <param name="Data">Массив байт, полученный из сети</param>
        /// <param name="EndPoint">Сетевой адрес - источник данных</param>
        private void OnDataReceived(byte[] Data, IPEndPoint EndPoint) => OnDataReceived(new DataReceivedEventArgs(Data, EndPoint));

        /* --------------------------------------------------------------------------------------------- */

        private readonly object _SyncRoot = new object();

        /// <summary>UDP-клиент</summary>
        private UdpClient _Client;

        /// <summary>Признак рабочего состояния</summary>
        private bool _Enabled;

        /// <summary>Номер сетевого порта</summary>
        private readonly int _Port = 0x5552;

        /// <summary>Делегат метода завершения асинхронной процедуры получения данных из сети</summary>
        private readonly AsyncCallback _EndReceive;

        protected CancellationTokenSource _ListenProcessCancellation;

        /* --------------------------------------------------------------------------------------------- */

        /// <summary>Признак рабочего состояния</summary>
        public virtual bool Enable { get => _Enabled; set { if (value) Start(); else Stop(); } }

        /// <summary>Номер сетевого порта</summary>
        public int Port => _Port;

        /* --------------------------------------------------------------------------------------------- */

        /// <summary>Инициализация нового объекта прослушивания порта (1025) UDP</summary>
        public Server() => _EndReceive = Listen;

        /// <summary>Инициализация нового объекта прослушивания порта UDP</summary>
        /// <param name="Port">Номер порта</param>
        public Server(int Port) : this() => _Port = Port;

        /* --------------------------------------------------------------------------------------------- */

        /// <summary>Процедура запуска процесса прослушивания порта</summary>
        protected virtual void Start()
        {
            if (_Enabled) return;
            lock (_SyncRoot)
                if (_Enabled) return;
                else
                {
                    _Enabled = true;
                    _Client = new UdpClient(_Port);
                    _ListenProcessCancellation = new CancellationTokenSource();
                    ListenAsync(_Client, _ListenProcessCancellation.Token);
                }
            OnConnected(EventArgs.Empty);
        }

        /// <summary>Процедура остановки процесса прослушивания порта</summary>
        protected virtual void Stop()
        {
            if (!_Enabled) return;
            lock (_SyncRoot)
                if (!_Enabled) return;
                else
                {
                    _Enabled = false;
                    _ListenProcessCancellation.Cancel();

                    _Client.Close();
                    _ListenProcessCancellation.Dispose();

                    _Client = null;
                    _ListenProcessCancellation = null;
                }
            OnDisconnected(EventArgs.Empty);
        }

        private async void ListenAsync(UdpClient Client, CancellationToken Cancel)
        {
            byte[] buffer = null;
            IPEndPoint address = null;
            while (_Enabled && !Cancel.IsCancellationRequested)
            {
                try
                {
                    var receive_data_task = Client.ReceiveAsync().WithCancellation(Cancel);

                    if(buffer != null)
                        OnDataReceived(buffer, address);

                    (buffer, address) = await receive_data_task.ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (SocketException)
                {
                    
                }
            }
        }

        /// <summary>Процедура прослушивания порта</summary>
        /// <param name="result">Аргумент асинхронной операции</param>
        protected virtual void Listen(IAsyncResult result)
        {
            if (!_Enabled) return;

            IPEndPoint end_point = null;
            var buffer = _Client.EndReceive(result, ref end_point);

            if (!_Enabled) return;
            if (buffer.Length > 0) OnDataReceived(buffer, end_point);

            _Client.BeginReceive(_EndReceive, null);
        }

        /* --------------------------------------------------------------------------------------------- */

        public int Send(byte[] Data) => _Client.Send(Data, Data.Length);

        public int Send(byte[] Data, IPEndPoint Address) => _Client.Send(Data, Data.Length, Address);

        public int Send(byte[] Data, string Host, int Port) => _Client.Send(Data, Data.Length, Host, Port);

        public async Task<int> SendAsync(byte[] Data, CancellationToken Cancel = default) => 
            await _Client.SendAsync(Data, Data.Length)
               .WithCancellation(Cancel)
               .ConfigureAwait(false);

        public async Task<int> SendAsync(byte[] Data, IPEndPoint Address, CancellationToken Cancel = default) => 
            await _Client.SendAsync(Data, Data.Length, Address)
               .WithCancellation(Cancel)
               .ConfigureAwait(false);

        public async Task<int> SendAsync(byte[] Data, string Host, int Port, CancellationToken Cancel = default) => 
            await _Client.SendAsync(Data, Data.Length, Host, Port)
               .WithCancellation(Cancel)
               .ConfigureAwait(false);

        /* --------------------------------------------------------------------------------------------- */

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _Disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _Disposed) return;
            lock (_SyncRoot)
            {
                Enable = false;
                _Client?.Dispose();
                _ListenProcessCancellation?.Dispose();
                _Disposed = true;
            }
        }

        #endregion
    }
}
