using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathCore.NET.TCP.Events;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable AssignmentIsFullyDiscarded

namespace MathCore.NET.TCP
{
    public class Client : IDisposable
    {
        /// <summary>Метод установления соединения у удалённым сервером</summary>
        /// <param name="Host">Адрес удалённого сервера</param>
        /// <param name="Port">Порт</param>
        public static Client Connect(string Host, int Port)
        {
            var client = new Client(Host, Port);
            client.Start();
            return client;
        }

        #region События

        /// <summary>Событие соединения клиента с сервером</summary>
        public event EventHandler Connected;

        /// <summary>Генерация события присоединения клиента к серверу</summary>
        protected virtual void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);

        /// <summary>Событие, возникающие при потери связи</summary>
        public event EventHandler Disconnected;

        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

        /// <summary>Событие, возникающие при ошибке</summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <param name="Error">Возникшая ошибка</param>
        protected virtual void OnError(Exception Error)
        {
            if (this.Error != null)
                this.Error.Invoke(this, new ErrorEventArgs(Error));
            else
                throw Error;
        }


        /// <summary>Событие, возникающие при появлении данных в сетевом потоке</summary>
        public event EventHandler<DataEventArgs> DataReceived;

        protected virtual void OnDataReceived(DataEventArgs e) => DataReceived?.Invoke(this, e);

        /// <param name="Data">Полученные данные</param>
        /// <param name="ReadedDataLength">Количество прочитанных байт</param>
        private void OnDataReceived(byte[] Data, int ReadedDataLength) => OnDataReceived(new DataEventArgs(Data, ReadedDataLength, _DataEncoding, _DataFormatter));

        /// <summary>Событие, возникающие при отправке данных</summary>
        public event EventHandler<DataEventArgs> DataSent;

        protected virtual void OnDataSent(DataEventArgs e) => DataSent?.Invoke(this, e);

        private void OnDataSent(byte[] Data) => OnDataSent(new DataEventArgs(Data, _DataEncoding, _DataFormatter));

        #endregion

        #region Поля

        private readonly object _SyncRoot = new object();

        protected TcpClient _Client;

        /// <summary>Сетевой поток данных</summary>
        protected NetworkStream _ClientStream;

        protected Encoding _DataEncoding = Encoding.UTF8;

        /// <summary>Строка с именем удалённого Host'а</summary>
        protected readonly string _Host = "127.0.0.1";

        /// <summary>Переменная целого типа, указывающий удалённый порт</summary>
        /// <remarks>В интерфейс класса не входит</remarks>
        protected readonly int _Port = 80;

        protected bool _Enabled;

        protected IFormatter _DataFormatter = new BinaryFormatter();

        private CancellationTokenSource _ConnectionCancellation;

        #endregion

        #region Свойства

        /// <summary>Свойство клиента, отражающее его статус</summary>
        public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }

        /// <summary>Удалённый Host</summary>
        public string Host => _Host;

        /// <summary>Удалённый порт</summary>
        public int Port => _Port;

        public Encoding DataEncoding { get => _DataEncoding; set => _DataEncoding = value; }

        public NetworkStream SocketStream => _ClientStream;

        public IFormatter DataFormatter
        {
            get => _DataFormatter;
            set => _DataFormatter = value ?? throw new NullReferenceException("Не задан стерилизатор!");
        }

        #endregion

        #region Конструктор / диструктор

        /// <remarks>
        /// Предполагается, что все необходимые параметры будут заданы во время работы с созданным объектом.
        /// Изначальные значения
        /// Host = "127.0.0.1"
        /// Port = 8080
        /// </remarks>
        public Client() { }

        public Client(string Host)
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new ArgumentException("На задан адрес хоста");
            const string tcp_scheme = "tcp://";
            if (Host.StartsWith(tcp_scheme))
                Host = Host.Substring(tcp_scheme.Length);

            var parts = Host.Split(':');
            _Host = parts[0];
            if (parts.Length <= 1 || !int.TryParse(parts[1], out _Port))
                throw new ArgumentException("Неверный формат строки. Должен быть: \"Host\", или \"Host\":Port");
        }

        /// <param name="Host">Адрес удалённого сервера</param>
        /// <param name="Port">Порт</param>
        public Client(string Host, int Port)
        {
            //Если значение порта недопустимо, то генерируем исключительную ситуацию
            if (Port < 1 || Port > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port), Port, $"Порт должен быть в пределах от 1 до 65535, а указан {Port}");
            _Host = Host;
            _Port = Port;
        }

        /// <summary>
        /// Конструктор клиента по указанному адресу сервера,
        /// порту и состоянию клиента после его создания.
        /// </summary>
        /// <param name="Host">Адрес удалённого сервера</param>
        /// <param name="Port">Порт</param>
        /// <param name="Enable">
        /// Состояние подключения после создания.
        /// True - клиент будет запущен непосредственно по завершении работы конструктора
        /// </param>
        public Client(string Host, int Port, bool Enable)
        {
            //Если значение порта недопустимо, то генерируем исключительную ситуацию
            if (Port < 1 || Port > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port), Port, $"Порт должен быть в пределах от 1 до 65535, а указан {Port}");
            _Host = Host;
            _Port = Port;
            if (Enable) Enabled = true;
        }

        /// <summary>Конструктор клиента по указанному объекту типа TcpClient</summary>
        /// <remarks>Нужен для использования в серверной части</remarks>
        /// <param name="Client">Объект класса TcpClient</param>
        public Client(TcpClient Client)
        {
            _Client = Client;

            //Создаём сетевой поток
            _ClientStream = new NetworkStream(_Client.Client);
            //_ClientStream.ReadTimeout = 250;

            //Выясняем удалённый адрес Host'а
            _Host = ((IPEndPoint)_Client.Client.RemoteEndPoint).Address.ToString();
            //Выясняем порт удалённого клиента
            _Port = ((IPEndPoint)_Client.Client.RemoteEndPoint).Port;

            _ConnectionCancellation = new CancellationTokenSource();
        }

        #endregion

        #region Запуск / остановка

        /// <summary>Запуска клиента</summary>
        public void Start()
        {
            if (_Enabled) return;
            lock (_SyncRoot)
                if (_Enabled) return;
                else
                {
                    if (_Client is null)
                    {
                        _Client = new TcpClient();
                        _ConnectionCancellation = new CancellationTokenSource();
                        try
                        {
                            _Client.Connect(_Host, _Port);
                        }
                        catch (SocketException error)
                        {
                            //Делаем непонятные телодвижения в плане анализ кодов ошибок... зачем - непонятно...
                            if (error.ErrorCode == 10061)
                                OnDisconnected();
                            OnError(error);
                            return;
                        }
                        _ClientStream = _Client.GetStream();
                    }
                    _Enabled = true;
                }
            CheckConnectionAsync(_ConnectionCancellation.Token);
            OnConnected();
        }

        /// <summary>Запуска клиента</summary>
        public async Task StartAsync()
        {
            //Если соединение уже установлено, то возврат
            if (_Enabled) return;
            Monitor.Enter(_SyncRoot);
            if (_Enabled)
            {
                Monitor.Exit(_SyncRoot);
                return;
            }

            if (_Client is null) _Client = new TcpClient();
            _ConnectionCancellation = new CancellationTokenSource();
            try
            {
                await _Client.ConnectAsync(_Host, _Port).ConfigureAwait(false);
                _ClientStream = _Client.GetStream();
                Monitor.Exit(_SyncRoot);
            }
            catch (SocketException error)
            {
                Monitor.Exit(_SyncRoot);
                // ReSharper disable once CommentTypo
                if (error.ErrorCode == 10061) //WSAECONNREFUSED = 10061
                    OnDisconnected();
                OnError(error);
                return;
            }

            _Enabled = true;
            CheckConnectionAsync(_ConnectionCancellation.Token);
            OnConnected();
        }

        /// <summary>Метод остановки клиента</summary>
        public void Stop()
        {
            //Если связь не была установлена, то возврат
            if (!_Enabled) return;
            lock (_SyncRoot)
                if (!_Enabled) return;
                else
                    try
                    {
                        //Сбрасываем флаг подключения
                        _Enabled = false;
                        _ConnectionCancellation.Cancel();
                        _ConnectionCancellation.Dispose();
                        _ConnectionCancellation = null;
                        //Начинаем асинхронный процесс отключения
                        _Client.Client.Disconnect(false);
                        _ClientStream.Close();
                        _ClientStream = null;
                        _Client = null;
                    }
                    catch (Exception error)
                    {
                        OnError(error);
                    }
            OnDisconnected();
        }

        /// <summary>Метод остановки клиента</summary>
        public async Task StopAsync()
        {
            //Если связь не была установлена, то возврат
            if (!_Enabled) return;
            Monitor.Enter(_SyncRoot);
            if (_Enabled)
            {
                Monitor.Exit(_SyncRoot);
                return;
            }

            try
            {
                _Enabled = false;
                _ConnectionCancellation.Cancel();
                _ConnectionCancellation.Dispose();
                _ConnectionCancellation = null;
                await _Client.Client.DisconnectAsync().ConfigureAwait(false);
                _ClientStream.Close();
                _ClientStream.Dispose();
                _Client.Dispose();
                _ClientStream = null;
                _Client = null;
                Monitor.Exit(_SyncRoot);
            }
            catch (Exception error)
            {
                Monitor.Exit(_SyncRoot);
                OnError(error);
                return;
            }
            OnDisconnected();
        }

        #endregion

        #region Отправка / получение

        protected async void CheckConnectionAsync(CancellationToken Cancel)
        {
            if (_ClientStream.CanRead)
            {
                var buffer_size = _Client.ReceiveBufferSize;
                var buffer = new byte[buffer_size];
                do
                {

                    if (Cancel.IsCancellationRequested) break;
                    var available_data = _Client.Available;
                    var readed_data = 0;
                    try
                    {
                        readed_data = await _ClientStream.ReadAsync(buffer, 0, available_data, Cancel)
                           .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { }
                    catch (IOException error)
                    {
                        OnError(error);
                    }
                    if (readed_data > 0) OnDataReceived(buffer, readed_data);
                }
                while (_ClientStream?.DataAvailable == true);
            }
            Stop();
        }

        /* ----------------------------------------------------------------------------------------------------------------------------------- */

        /// <summary>Метод отправки данных на сервер</summary>
        /// <param name="Message">Отправляемые данные</param>
        public void Send(string Message) => Send(DataEncoding.GetBytes(Message));

        public async Task SendAsync(string Message, CancellationToken Cancel = default) =>
            await SendAsync(DataEncoding.GetBytes(Message), Cancel).ConfigureAwait(false);

        public void Send(byte[] Data)
        {
            if (Data is null) throw new ArgumentNullException(nameof(Data));
            if (Data.Length == 0) return;

            if (!_Enabled) return;
            try
            {
                var stream = _ClientStream;
                lock (stream)
                    stream.Write(Data, 0, Data.Length);
                OnDataSent(Data);
            }
            catch (SocketException error)
            {
                if (error.ErrorCode == 10053)
                {
                    Stop();
                    OnDisconnected();
                }
                else
                {
                    Stop();
                    OnError(error);
                }
            }
            catch (IOException)
            {
                Stop();
                OnDisconnected();
            }
            catch (Exception error)
            {
                OnError(error);
            }
        }

        public async Task SendAsync(byte[] Data, CancellationToken Cancel = default)
        {
            if (Data is null) throw new ArgumentNullException(nameof(Data));

            if (Data.Length == 0 || !_Enabled) return;

            var cancel = CancellationTokenSource
               .CreateLinkedTokenSource(_ConnectionCancellation.Token, Cancel).Token;
            cancel.ThrowIfCancellationRequested();

            try
            {
                var stream = _ClientStream;
                Monitor.Enter(stream);
                try
                {
                    await stream.WriteAsync(Data, 0, Data.Length, cancel).ConfigureAwait(false);
                }
                finally
                {
                    Monitor.Exit(stream);
                }
                OnDataSent(Data);
            }
            catch (SocketException error)
            {
                if (error.ErrorCode == 10053)
                {
                    Stop();
                    OnDisconnected();
                }
                else
                {
                    Stop();
                    OnError(error);
                }
            }
            catch (IOException)
            {
                Stop();
                OnDisconnected();
            }
            catch (Exception error)
            {
                OnError(error);
            }
        }

        public void SendObject(object Object)
        {
            if (Object is null || !_Enabled) return;
            var stream = _ClientStream;
            lock (stream) DataFormatter.Serialize(stream, Object);
        }

        public async Task SendObjectAsync(object Object, CancellationToken Cancel = default)
        {
            if (Object is null || !_Enabled) return;
            var stream = _ClientStream;
            Monitor.Enter(stream);
            try
            {
                await Task.Run(() => DataFormatter.Serialize(stream, Object), Cancel)
                   .ConfigureAwait(false);
            }
            finally
            {
                Monitor.Exit(stream);
            }
        }

        #endregion

        public override string ToString() => $"{Host}:{Port}";

        public static explicit operator TcpClient(Client Client) => Client._Client;

        public static explicit operator Client(TcpClient Client) => new Client(Client);

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
            _ConnectionCancellation?.Dispose();
            _Client?.Dispose();
            _ClientStream?.Dispose();
        }

        #endregion
    }
}