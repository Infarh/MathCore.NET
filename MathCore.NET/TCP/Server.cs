using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MathCore.NET.TCP.Events;

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
        public event EventHandler Stoped;

        /// <summary>Метод вызова события "при остановке сервера"</summary>
        protected virtual void OnStoped(EventArgs e) => Stoped?.Invoke(this, e);
        private void OnStoped() => OnStoped(EventArgs.Empty);

        /// <summary>Событие, возникающие при подключении нового клиента</summary>
        public event EventHandler<ClientEventArgs> ClientConnected;

        /// <summary>Метод вызова события "при подключении клиента"</summary>
        /// <param name="e">Подключившийся клиент</param>
        protected virtual void OnClientConnected(ClientEventArgs e) =>
            ClientConnected?.Invoke(this, e);

        private void OnClientConnected(object Sender) =>
            OnClientConnected(new ClientEventArgs((Client)Sender));

        /// <summary>Событие, возникающие при отключении клиента</summary>
        public event EventHandler<ClientEventArgs> ClientDisconnected;

        /// <summary>Метод вызова события "при отключении клиента"</summary>
        /// <param name="e">Отключившийся клиент</param>
        protected virtual void OnClientDisconnected(ClientEventArgs e) =>
            ClientDisconnected?.Invoke(this, e);

        private void OnClientDisconnected(object Sender) =>
            OnClientDisconnected(new ClientEventArgs((Client)Sender));

        /// <summary>Событие, возникающие при получении данных подключённым клиентом</summary>
        public event EventHandler<ClientDataEventArgs> DataReceived;

        /// <summary>Метод вызова события "при получении данных"</summary>
        protected virtual void OnDataReceived(ClientDataEventArgs e) => DataReceived?.Invoke(this, e);

        /// <param name="Sender">Клиент, получивший данные</param>
        /// <param name="ClientArgs">Полученные данные (надо переписать, поменяв на полученный от клиента аргумент соответствующего события)</param>
        private void OnDataReceived(object Sender, DataEventArgs ClientArgs) =>
            OnDataReceived(new ClientDataEventArgs((Client)Sender, ClientArgs));

        /// <summary>Событие, возникающие при отправке данных подключённым клиентом</summary>
        public event EventHandler<ClientDataEventArgs> DataSended;

        /// <summary>Метод вызова события "при передаче данных"</summary>
        protected virtual void InvokeDataSendEvent(ClientDataEventArgs e) => DataSended?.Invoke(this, e);

        /// <param name="Sender">Клиент, инициировавший передачу</param>
        /// <param name="ClientArgs">Параметры передачи</param>
        private void InvokeDataSendEvent(object Sender, DataEventArgs ClientArgs) =>
            InvokeDataSendEvent(new ClientDataEventArgs((Client)Sender, ClientArgs));

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
        protected int _Port;

        protected readonly Encoding _DataEncoding = Encoding.UTF8;

        /// <summary>Тип прослушиваемых IP адресов</summary>
        protected readonly IPAddress _AddressType = IPAddress.Any;

        /// <summary>Основной элемент сервера, производящий прослушивание порта и подключения входящих клиентов</summary>
        protected TcpListener _Listner;

        /// <summary>Главный поток сервера</summary>
        protected Thread _ServerThread;

        /// <summary>Поле, содержащее информацию о активности сервера</summary>
        protected bool _Enabled;

        /// <summary>Mutex синхронизации процесса асинхронного подключения клиентов</summary>
        protected ManualResetEvent _ClientConnection;

        /// <summary>Список подключённых клиентов</summary>
        protected List<Client> _ClientList;

        #endregion

        #region Свойства

        /// <summary>Свойство отражает состояние сервера</summary>
        /// <value>подключён / отключён</value>
        public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }

        /// <summary>Свойство, указывающее прослушиваемый порт</summary>
        /// <value>Должно быть целым в пределах от 1 до 2^16</value>
        public int Port
        {
            get => _Port;
            set
            {
                if (value == _Port) return;

                if (value < 1 || value > 65535)
                    throw new ArgumentOutOfRangeException(nameof(Port), value, $"Порт должен быть в пределах от 1 до 65535, а указан {value}");

                var is_enabled = Enabled;
                Enabled = false;
                _Port = value;
                Enabled = is_enabled;
            }
        }

        #endregion

        #region Конструктор / диструктор

        /// <summary>Конструктор с указанием прослушиваемого порта</summary>
        /// <param name="Port">Прослушиваемый порт</param>
        public Server(int Port)
        {
            if (Port < 1 || Port > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port), Port, "Порт должен быть в пределах от 1 до 65535, а указан " + Port.ToString());
            _Port = Port;
        }

        /// <summary>Конструктор с указанием прослушиваемого порта и типа обслуживаемых подсетей</summary>
        /// <param name="Port">Прослушиваемый порт</param>
        /// <param name="AddressType">Система IP адресов</param>
        public Server(int Port, IPAddress AddressType)
        {
            if (Port < 1 || Port > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port), Port, "Порт должен быть в пределах от 1 до 65535, а указан " + Port.ToString());
            _Port = Port;
            _AddressType = AddressType;
        }

        #endregion

        #region Запуск / остановка

        /// <summary>Метод запуска сервера</summary>
        protected void Start()
        {
            //Если сервер активен, выходим
            if (Enabled) return;
            lock (_SyncRoot)
            {
                if (Enabled) return;
                try
                {
                    //Устанавливаем признак активности сервера
                    _Enabled = true;

                    //Создаём новый экземпляр "слушателя"
                    _Listner = new TcpListener(_AddressType, _Port);
                    _Listner.Start();
                    //Создаём блокирующий объект для синхронизации потоков
                    _ClientConnection = new ManualResetEvent(false);
                    //Создаём список обслуживаемых клиентов
                    _ClientList = new List<Client>(10);

                    //Создаём основной поток обработки подключений
                    _ServerThread = new Thread(Listen);
                    //Запускаем сервер
                    _ServerThread.Start();

                    OnStarted();
                }
                catch (SocketException error)
                {
                    Stop();
                    InvokeErrorEvent(error);
                }
            }
        }

        /// <summary>Метод остановки сервера</summary>
        protected void Stop()
        {
            //Если сервер неактивен, то выходим
            if (!Enabled) return;
            lock (_SyncRoot)
            {
                if (!Enabled) return;
                _Enabled = false;

                if (_ClientList != null)
                    for (var i = 0; i < _ClientList.Count; i++)
                        _ClientList[i].Enabled = false;

                //Останавливаем слушателя
                _Listner.Stop();
                _ClientConnection?.Set();

                //Если поток всё ещё активен, то...
                if (_ServerThread?.IsAlive == true)
                {
                    //Вызываем остановку потока вручную.
                    _ServerThread.Abort();
                    //Ожидаем завершения потока (синхронизация)
                    _ServerThread.Join();
                }

                //Обнуляем ссылки
                _ServerThread = null;
                _Listner = null;
                _ClientConnection = null;
            }

            //Устанавливаем признак активности сервера в состояние "отключён"

            OnStoped();
        }

        #endregion

        #region Обработка подключений

        /// <summary>Главный метод сервера.</summary>
        /// <remarks>
        /// В нём производиться асинхронная обработка подключений клиентов
        /// Стартует в отдельном потоке
        /// </remarks>
        protected void Listen()
        {
            //Основной цикл сервера. Предназначен для определения факта подключения
            while (_Enabled) //Признак продолжения работы - активность сервера
            {
                try //Блок обработки ошибок
                {
                    _ClientConnection.Reset();
                    //Асинхронный захват подключения с передачей дальнейшей обработки в метод void ClientAccepted(IAsyncResult AsyncResult)
                    _Listner.BeginAcceptTcpClient(ClientAccepted, _Listner);
                    //Ожидаем флага разрешения дальнейшей работы
                    _ClientConnection.WaitOne();
                }
                catch (Exception error) //Перехват остальных ошибок
                {
                    InvokeErrorEvent(error);
                }
            }
        }

        /// <summary>Метод асинхронного завершения подлючения клиента</summary>
        protected void ClientAccepted(IAsyncResult AsyncResult)
        {
            var listener = (TcpListener)AsyncResult.AsyncState;

            if (!Enabled) return;

            //Обработка исключительных ситуаций (какая умная фраза!)
            try
            {
                //Получаем подключившегося клиента
                InitializeClient(listener.EndAcceptTcpClient(AsyncResult));
            }
            catch (Exception error)
            {
                InvokeErrorEvent(error);
            }
            //Разрешение на дальнейшую обработку подключений клиента
            _ClientConnection.Set();
        }

        protected virtual void InitializeClient(TcpClient Client)
        {
            //Создаём новый экземпляр класса "Client", в котором будет происходить дальнейшая работа с клиентом
            var client = new Client(Client);

            //подписываемся на обработчики событий клиента
            client.Connected += OnClientConnected;
            client.Disconnected += OnClientDisconnected;
            client.DataReceived += OnClientDataReceived;
            client.Error += OnClientError;
            client.DataSended += OnClientDataSended;

            client.DataEncoding = _DataEncoding;

            //Добавляем клиента в список
            _ClientList.Add(client);
            OnClientConnected(client);
        }

        #endregion

        #region Обработка событий подключённых клиентов

        /// <summary>Метод обработки событий подключённых клиентов "при отправке данных"</summary>
        /// <param name="Sender">Клиент, отправивший данные</param>
        /// <param name="Args">Параметры</param>
        private void OnClientDataSended(object Sender, DataEventArgs Args) =>
            InvokeDataSendEvent(Sender, Args);

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
            OnDataReceived(Sender, Args);

        /// <summary>Метод обработки событий подключённых клиентов "при отключении"</summary>
        /// <param name="Sender">Отключившийся клиент</param>
        /// <param name="Args">Параметры</param>
        private void OnClientDisconnected(object Sender, EventArgs Args)
        {
            var client = (Client)Sender;
            _ClientList.Remove(client);
            OnClientDisconnected(client);
        }

        /// <summary>Метод обработки событий подключённых клиентов "при подключении"</summary>
        /// <param name="Sender">Подключившийся клиент</param>
        /// <param name="Args">Параметры</param>
        private void OnClientConnected(object Sender, EventArgs Args) => OnClientConnected(Sender);

        #endregion

        public override string ToString() => $"TCP:{Port}";

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
        }

        #endregion

    }
}
