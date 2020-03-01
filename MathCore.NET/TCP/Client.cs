using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using MathCore.NET.TCP.Events;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace MathCore.NET.TCP
{
    public class Client : IDisposable
    {
        #region События

        /// <summary>Событие соединения клиента с сервером</summary>
        public event EventHandler Connected;

        /// <summary>Позволяет легко вызвать событие "при присоединении"</summary>
        protected virtual void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);

        /// <summary>Событие, возникающие при потери связи</summary>
        public event EventHandler Disconnected;

        protected virtual void InvokeDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);

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
        private void OnDataReceived(byte[] Data) => OnDataReceived(new DataEventArgs(Data, _DataEncoding, _DataFormatter));

        /// <summary>Событие, возникающие при отправке данных</summary>
        public event EventHandler<DataEventArgs> DataSent;

        protected virtual void OnDataSent(DataEventArgs e) => DataSent?.Invoke(this, e);

        private void OnDataSent(byte[] Data) => OnDataSent(new DataEventArgs(Data, _DataEncoding, _DataFormatter));

        #endregion

        #region Поля

        protected TcpClient _Client;

        /// <summary>Сетевой поток данных</summary>
        protected NetworkStream _ClientStream;
        protected BinaryReader _Reader;
        protected BinaryWriter _Writer;

        protected Encoding _DataEncoding = Encoding.ASCII;

        /// <summary>Строка с именем удалённого Host'а</summary>
        protected string _Host = "127.0.0.1";

        /// <summary>Переменная целого типа, указывающий удалённый порт</summary>
        /// <remarks>В интерфейс класса не входит</remarks>
        protected int _Port = 80;
        protected Thread _CheckThread;

        protected bool _Enabled;

        protected IFormatter _DataFormatter = new BinaryFormatter();

        #endregion

        #region Свойства

        /// <summary>Свойство клиента, отражающее его статус</summary>
        public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }

        /// <summary>Удалённый Host</summary>
        /// <remarks>
        /// Можно изменять во время работы клиента. Клиент отключится от сервера,
        /// изменит настройку и попытается переподключиться к указанному Host'у
        /// </remarks>
        /// <value>Имеет строковый тип. Может быть указан в виде IP адреса.</value>
        public string Host
        {
            get => _Host;
            set
            {
                if (value == _Host) return;
                var is_enabled = Enabled;
                Enabled = false;
                _Host = value;
                Enabled = is_enabled;
            }
        }

        /// <summary>Удалённый порт</summary>
        /// <remarks>
        /// В случае задания недопустимого значения будет сгенерировано исключение
        /// Может быть изменено во время работы клиента. Клиент отключиться от сервера, изменит настройку и попытается установить соединение с новым сервером.
        /// </remarks>
        /// <value>принимает значения целого типа в пределах от 1 до 2^16</value>
        public int Port
        {
            get => _Port;
            set
            {
                //Если пытаемся установить текущее значение порта, то возврат
                if (value == _Port) return;
                //Если значение недопустимо, то генерируем исключение
                if (value < 1 || value > 65535)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Порт должен быть в пределах от 1 до 65535, а указан " + value.ToString());

                var is_enabled = Enabled;
                Enabled = false;
                _Port = value;
                Enabled = is_enabled;
            }
        }

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
            _Enabled = true;
            //проверяем, получен ли от сервера подключённый клиент.
            //Если соединение отсутствует, то возврат
            if (!Enabled) return;

            //Создаём сетевой поток
            _ClientStream = new NetworkStream(_Client.Client);
            //ClientStream.ReadTimeout = 250;
            //...объект для чтения данных из потока
            _Reader = new BinaryReader(_ClientStream);
            //...объект для записи данных в поток
            _Writer = new BinaryWriter(_ClientStream);

            //Выясняем удалённый адрес Host'а
            _Host = _Client.Client.RemoteEndPoint.ToString().Split(':')[0];
            //Выясняем порт удалённого клиента
            _Port = int.Parse(_Client.Client.RemoteEndPoint.ToString().Split(':')[1]);

            //Создаём основной поток клиента
            _CheckThread = new Thread(CheckConnection);
            //Запускаем основной поток клиента
            _CheckThread.Start();
        }

        #endregion

        #region Запуск / остановка

        /// <summary>Метод установления соединения у удалённым сервером</summary>
        /// <param name="HostName">Адрес удалённого сервера</param>
        /// <param name="Port">Порт</param>
        // ReSharper disable once ParameterHidesMember
        public bool Connect(string HostName, int Port)
        {
            //Если подключение к указанному адресу и порту уже выполнено, то возврат
            if (_Enabled && HostName == _Host && Port == _Port) return true;

            //В случае, если клиент был подключён, отключаемся
            Enabled = false;

            //Устанавливаем новые параметры соединения
            this.Host = HostName;
            this.Port = Port;

            //Подключаемся к новой точке
            Enabled = true;

            return _Enabled;
        }

        /// <summary>Метод запуска клиента</summary>
        protected void Start()
        {
            //Если соединение уже установлено, то возврат
            if (Enabled) return;

            try
            {
                //Устанавливаем флаг, свидетельствующий о наличии соединения
                _Enabled = true;

                //Создаём нового клиента
                if (_Client == null) _Client = new TcpClient();
                //Подключаем клиента к удалённому серверу
                _Client.Connect(_Host, _Port);

                //Получаем сетевой поток
                _ClientStream = _Client.GetStream();
                //Создаём объект для чтения из сетевого потока
                _Reader = new BinaryReader(_ClientStream);
                //...и для записи в сетевой поток
                _Writer = new BinaryWriter(_ClientStream);

                //Создаём основной поток клиента
                _CheckThread = new Thread(CheckConnection);
                //Запускаем основной поток клиента
                _CheckThread.Start();

                //сообщаем о подключении
                OnConnected();
            }
            //В случае возникновения ошибки сокета
            catch (SocketException error)
            {
                //Сбрасываем флаг подключения
                _Enabled = false;
                //Делаем непонятные телодвижения в плане анализ кодов ошибок... зачем - непонятно...
                if (error.ErrorCode == 10061)
                    InvokeDisconnected();
                OnError(error);
            }
        }

        /// <summary>Метод остановки клиента</summary>
        protected void Stop()
        {
            //Если связь не была установлена, то возврат
            if (!Enabled) return;
            //Сбрасываем флаг подключения
            _Enabled = false;

            //Начинаем асинхронный процесс отключения
            _Client.Client.BeginDisconnect(false, DisconnectionComplicate, _Client.Client);
        }

        /// <summary>Метод асинхронного завершения процесса отключения от сервера</summary>
        protected void DisconnectionComplicate(IAsyncResult AsyncResult)
        {
            //Получаем сокет клиента
            var socket = (Socket)AsyncResult.AsyncState;
            try
            {
                //Пытаемся его отключить
                socket.EndDisconnect(AsyncResult);

                //Если главный поток клиента ещё активен
                if (_CheckThread != null)
                {
                    _Reader.Close();
                    if (_CheckThread.IsAlive)
                        _CheckThread.Abort(); //Останавливаем поток
                    _CheckThread.Join(); //Ожидаем завершения
                    _CheckThread = null; //Убираем ссылку на главный поток (отдаём его на растерзание сборщику мусора)
                }

                //Закрываем поток данных
                _ClientStream.Close();

                _Reader = null;
                _Writer = null;

                _ClientStream = null;

                //Убираем ссылку на клиента
                _Client = null;

                //Сообщаем об отключении
                InvokeDisconnected();

            }
            //В случае ошибки
            catch (Exception error)
            {
                OnError(error);
            }
        }

        #endregion

        #region Отправка / получение

        /// <summary>Главный метод клиента. Стартует в отдельном потоке. Проверяет наличие данных от сервера и связь с ним.</summary>
        protected void CheckConnection()
        {
            OnConnected();

            //Читаем из потока до тех пор, пока не будет сброшен флаг подключения (сверху не решат отключиться)
            //или пока не будет получено "ничто" - это происходит, когда удалённый сервер порвал соединение
            try
            {

                var buffer = new byte[1];
                var @continue = true;
                while (Enabled && (buffer.Length != 0 || @continue))
                {
                    buffer = _Reader.ReadBytes(_Client.Available);
                    @continue = _Client.Available != 0;
                    if (buffer.Length > 0)
                        OnDataReceived(buffer);
                }
            }
            catch (ThreadAbortException)
            { }
            catch (IOException)
            { }

            //После выпадения из цикла (значит соединение было разорвано) вызываем метод отключения клиента.
            //Просто так его вызвать нельзя потому, что тогда поток будет ждать его завершения, а в это метода
            //происходит остановка этого потока с его синхронизацией. В итоге происходит мёртвая блокировка.
            //Во избежании этого мы запускаем метод отключения в отдельном потоке, а этот благополучно завершается.
            new Thread(Stop).Start();

            //В случае если соединение рвётся на нашей стороне по команде пользователя, то метод отключения уже инициирован, 
            //но мы его запускаем ещё раз. Это делать можно потому, что поток будет остановлен тем методом в момент
            //когда уже будет сброшен флаг активности клиента и это не позволит повторно войти в метод остановки.
        }

        /* ----------------------------------------------------------------------------------------------------------------------------------- */

        /// <summary>Метод отправки данных на сервер</summary>
        /// <param name="Message">Отправляемые данные</param>
        public void Send(string Message) => Send(DataEncoding.GetBytes(Message));

        public void Send(byte[] Data)
        {
            if (!Enabled) return;
            try
            {
                lock (_ClientStream)
                {
                    _Writer.Write(Data);
                    _Writer.Flush();
                }
                OnDataSent(Data);
            }
            catch (SocketException error)
            {
                if (error.ErrorCode == 10053)
                {
                    Stop();
                    InvokeDisconnected();
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
                InvokeDisconnected();
            }
            catch (Exception error)
            {
                OnError(error);
            }
        }

        public void SendObject(object Object)
        {
            lock (_ClientStream) DataFormatter.Serialize(_ClientStream, Object);
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
            _Client?.Dispose();
            _ClientStream?.Dispose();
            _Reader?.Dispose();
            _Writer?.Dispose();
        }

        #endregion
    }
}