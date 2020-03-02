using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathCore.NET.TCP.Events;
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace MathCore.NET.TCP
{
    public class Server : IDisposable
    {
        #region События

        /// <summary>Событие, возникающие при запуске сервера</summary>
        public event EventHandler Started;

        /// <summary>Метод вызова события "при запуске сервера"</summary>
        protected virtual void OnStarted(EventArgs e) => Started?.Invoke(this, e);

        private void OnStarted() => OnStarted(EventArgs.Empty);

        /// <summary>Событие, возникающие при остановке сервера</summary>
        public event EventHandler Stopped;

        /// <summary>Метод вызова события "при остановке сервера"</summary>
        protected virtual void OnStopped(EventArgs e) => Stopped?.Invoke(this, e);
        private void OnStopped() => OnStopped(EventArgs.Empty);

        /// <summary>Событие, возникающие при подключении нового клиента</summary>
        public event EventHandler<ClientEventArgs> ClientConnected;

        /// <summary>Метод вызова события "при подключении клиента"</summary>
        /// <param name="e">Подключившийся клиент</param>
        protected virtual void OnClientConnected(ClientEventArgs e) =>
            ClientConnected?.Invoke(this, e);

        private void OnClientConnected(Client Client) =>
            OnClientConnected(new ClientEventArgs(Client));

        /// <summary>Событие, возникающие при отключении клиента</summary>
        public event EventHandler<ClientEventArgs> ClientDisconnected;

        /// <summary>Метод вызова события "при отключении клиента"</summary>
        /// <param name="e">Отключившийся клиент</param>
        protected virtual void OnClientDisconnected(ClientEventArgs e) =>
            ClientDisconnected?.Invoke(this, e);

        private void OnClientDisconnected(Client Client) =>
            OnClientDisconnected(new ClientEventArgs(Client));

        /// <summary>Событие, возникающие при получении данных подключённым клиентом</summary>
        public event EventHandler<ClientDataEventArgs> DataReceived;

        /// <summary>Метод вызова события "при получении данных"</summary>
        protected virtual void OnDataReceived(ClientDataEventArgs e) => DataReceived?.Invoke(this, e);

        /// <param name="Client">Клиент, получивший данные</param>
        /// <param name="ClientArgs">Полученные данные (надо переписать, поменяв на полученный от клиента аргумент соответствующего события)</param>
        private void OnDataReceived(Client Client, DataEventArgs ClientArgs) =>
            OnDataReceived(new ClientDataEventArgs(Client, ClientArgs));

        /// <summary>Событие, возникающие при отправке данных подключённым клиентом</summary>
        public event EventHandler<ClientDataEventArgs> DataSent;

        /// <summary>Метод вызова события "при передаче данных"</summary>
        protected virtual void InvokeDataSendEvent(ClientDataEventArgs e) => DataSent?.Invoke(this, e);

        /// <param name="Client">Клиент, инициировавший передачу</param>
        /// <param name="ClientArgs">Параметры передачи</param>
        private void InvokeDataSendEvent(Client Client, DataEventArgs ClientArgs) =>
            InvokeDataSendEvent(new ClientDataEventArgs(Client, ClientArgs));

        /// <summary>Событие, возникающие при возникновении ошибки</summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>Метод вызова события "при ошибке"</summary>
        /// <param name="Error">Возникшая ошибка</param>
        protected virtual void InvokeErrorEvent(Exception Error)
        {
            var handler = this.Error;
            if (handler != null)
                handler.Invoke(this, new ErrorEventArgs(Error));
            else
                throw Error;
        }

        #endregion

        #region Поля

        private readonly object _SyncRoot = new object();

        /// <summary>Поле, содержащее текущий прослушиваемый порт</summary>
        protected readonly int _Port;

        protected readonly Encoding _DataEncoding = Encoding.UTF8;

        /// <summary>Тип прослушиваемых IP адресов</summary>
        protected readonly IPAddress _AddressType = IPAddress.Any;

        /// <summary>Основной элемент сервера, производящий прослушивание порта и подключения входящих клиентов</summary>
        protected TcpListener _Listener;

        private CancellationTokenSource _ListenProcessCancellation;

        /// <summary>Поле, содержащее информацию о активности сервера</summary>
        protected bool _Enabled;

        /// <summary>Список подключённых клиентов</summary>
        protected List<Client> _Clients;

        #endregion

        #region Свойства

        /// <summary>Свойство отражает состояние сервера</summary>
        /// <value>подключён / отключён</value>
        public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }

        /// <summary>Прослушиваемый порт</summary>
        public int Port => _Port;

        /// <summary>
        /// Система IP адресов</summary>
        public IPAddress AddressType => _AddressType;

        #endregion

        #region Конструктор / диструктор

        /// <summary>Конструктор с указанием прослушиваемого порта</summary>
        /// <param name="Port">Прослушиваемый порт</param>
        public Server(int Port) => _Port = Port < 1 || Port > 65535
            ? throw new ArgumentOutOfRangeException(nameof(Port), Port,
                $"Порт должен быть в пределах от 1 до 65535, а указан {Port}")
            : Port;

        /// <summary>Конструктор с указанием прослушиваемого порта и типа обслуживаемых подсетей</summary>
        /// <param name="Port">Прослушиваемый порт</param>
        /// <param name="AddressType">Система IP адресов</param>
        public Server(int Port, IPAddress AddressType) : this(Port) => _AddressType = AddressType;

        #endregion

        #region Запуск / остановка

        /// <summary>Метод запуска сервера</summary>
        public void Start()
        {
            //Если сервер активен, выходим
            if (_Enabled) return;
            lock (_SyncRoot)
                if (!_Enabled)
                    try
                    {
                        //Устанавливаем признак активности сервера
                        _Enabled = true;

                        //Создаём новый экземпляр "слушателя"
                        _Listener = new TcpListener(_AddressType, _Port);
                        _Listener.Start();
                        //Создаём список обслуживаемых клиентов
                        _Clients = new List<Client>();

                        _ListenProcessCancellation = new CancellationTokenSource();
                        ListenAsync(_Listener, _ListenProcessCancellation.Token);

                    }
                    catch (SocketException error)
                    {
                        Stop();
                        InvokeErrorEvent(error);
                    }
                else return;
            OnStarted();
        }

        /// <summary>Метод остановки сервера</summary>
        public void Stop()
        {
            //Если сервер неактивен, то выходим
            if (!_Enabled) return;
            lock (_SyncRoot)
                if (_Enabled)
                {
                    //Устанавливаем признак активности сервера в состояние "отключён"
                    _Enabled = false;
                    _ListenProcessCancellation.Cancel();
                    _ListenProcessCancellation.Dispose();

                    //Останавливаем слушателя
                    _Listener.Stop();

                    _Clients?.ForEach(client => client.Stop());
                    _Clients?.Clear();

                    //Обнуляем ссылки
                    _Listener = null;
                    _ListenProcessCancellation = null;
                }
                else return;

            OnStopped();
        }

        #endregion

        #region Обработка подключений

        protected virtual async void ListenAsync(TcpListener Listener, CancellationToken Cancel)
        {
            try
            {
                TcpClient client = null;
                while (true)
                {
                    Cancel.ThrowIfCancellationRequested();
                    var waiting_client_task = Listener
                       .AcceptTcpClientAsync()
                       .WithCancellation(Cancel)
                       .ConfigureAwait(false);

                    if (client != null) AcceptClient(client);

                    client = await waiting_client_task;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                InvokeErrorEvent(error);
            }
        }

        protected virtual void AcceptClient(TcpClient Client)
        {
            //Создаём новый экземпляр класса "Client", в котором будет происходить дальнейшая работа с клиентом
            var client = new Client(Client);

            //подписываемся на события клиента
            client.Disconnected += OnClientDisconnected;
            client.DataReceived += OnClientDataReceived;
            client.Error += OnClientError;
            client.DataSent += OnClientDataSent;

            client.DataEncoding = _DataEncoding;

            //Добавляем клиента в список
            lock (_SyncRoot) _Clients.Add(client);

            client.Start();

            OnClientConnected(client);
        }

        protected virtual void DisconnectClient(Client Client)
        {
            //отписываемся от событий клиента
            Client.Disconnected -= OnClientDisconnected;
            Client.DataReceived -= OnClientDataReceived;
            Client.Error -= OnClientError;
            Client.DataSent -= OnClientDataSent;

            //Удаляем клиента из списка
            lock (_SyncRoot) _Clients.Remove(Client);
            OnClientDisconnected(Client);
        }

        #endregion

        #region Обработка событий подключённых клиентов

        /// <summary>Метод обработки событий подключённых клиентов "при отправке данных"</summary>
        /// <param name="Sender">Клиент, отправивший данные</param>
        /// <param name="Args">Параметры</param>
        private void OnClientDataSent(object Sender, DataEventArgs Args) =>
            InvokeDataSendEvent((Client)Sender, Args);

        /// <summary>Метод обработки событий подключённых клиентов "при ошибке"</summary>
        /// <param name="Sender">Клиент, совершивший ошибку</param>
        /// <param name="Args">Параметры</param>
        private void OnClientError(object Sender, ErrorEventArgs Args) =>
            //Передаётся, как ошибка сервера (Не совсем корректно. Надо передавать так же в виде параметра самого виновника ошибки)
            InvokeErrorEvent(Args.GetException());

        /// <summary>Метод обработки событий подключённых клиентов "при получении данных"</summary>
        /// <param name="Sender">Клиент, получивший данные</param>
        /// <param name="Args">Параметры</param>
        private void OnClientDataReceived(object Sender, DataEventArgs Args) =>
            OnDataReceived((Client)Sender, Args);

        /// <summary>Метод обработки событий подключённых клиентов "при отключении"</summary>
        /// <param name="Sender">Отключившийся клиент</param>
        /// <param name="Args">Параметры</param>
        private void OnClientDisconnected(object Sender, EventArgs Args) => DisconnectClient((Client)Sender);

        #endregion

        public override string ToString() => $"tcp://{_AddressType}:{Port}";

        #region IDispose implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Признак того, что объект был разрушен</summary>
        private bool _Disposed;

        /// <summary>Освобождение ресурсов</summary>
        /// <param name="disposing">Выполнить освобождение управляемых ресурсов</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed || !disposing) return;
            _Disposed = true;
            Stop();
            _ListenProcessCancellation?.Dispose();
            lock (_SyncRoot) _Clients?.Clear();
        }

        #endregion
    }
}
