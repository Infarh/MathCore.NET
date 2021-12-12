using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using MathCore.NET.Samples.TCP.Client.Services.Interfaces;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;

namespace MathCore.NET.Samples.TCP.Client.ViewModels
{
    class MainWindowViewModel : ViewModel
    {
        private readonly ITCPClient _Client;

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Клиент";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        #region Host : string - Адрес хоста

        /// <summary>Адрес хоста</summary>
        private string _Host = "127.0.0.1";

        /// <summary>Адрес хоста</summary>
        public string Host { get => _Host; set => Set(ref _Host, value); }

        #endregion

        #region Port : int - Номер порта

        /// <summary>Номер порта</summary>
        private int _Port = 8080;

        /// <summary>Номер порта</summary>
        public int Port { get => _Port; set => Set(ref _Port, value); }

        #endregion

        #region Message : string - Сообщение

        /// <summary>Сообщение</summary>
        private string _Message;

        /// <summary>Сообщение</summary>
        public string Message { get => _Message; set => Set(ref _Message, value); }

        #endregion

        public ObservableCollection<IncomingMessage> Messages { get; } = new();

        #region Команды

        #region Command ConnectCommand - Подключиться к хосту

        /// <summary>Подключиться к хосту</summary>
        public ICommand ConnectCommand { get; }

        /// <summary>Проверка возможности выполнения - Подключиться к хосту</summary>
        private bool CanConnectCommandExecute() => !_Client.Connected;

        /// <summary>Логика выполнения - Подключиться к хосту</summary>
        private void OnConnectCommandExecuted() => _Client.Connect(_Host, Port);

        #endregion

        #region Command DisconnectCommand - Отключиться от хоста

        /// <summary>Отключиться от хоста</summary>
        public ICommand DisconnectCommand { get; }

        /// <summary>Проверка возможности выполнения - Отключиться от хоста</summary>
        private bool CanDisconnectCommandExecute() => _Client.Connected;

        /// <summary>Логика выполнения - Отключиться от хоста</summary>
        private void OnDisconnectCommandExecuted() => _Client.Disconnect();

        #endregion

        #region Command SendMessageCommand : string - Отправка сообщения

        /// <summary>Отправка сообщения</summary>
        private ICommand _SendMessageCommand;

        /// <summary>Отправка сообщения</summary>
        public ICommand SendMessageCommand => _SendMessageCommand
            ??= new LambdaCommand<string>(OnSendMessageCommandExecuted, CanSendMessageCommandExecute);

        /// <summary>Проверка возможности выполнения - Отправка сообщения</summary>
        private bool CanSendMessageCommandExecute(string p) => _Client.Connected;

        /// <summary>Проверка возможности выполнения - Отправка сообщения</summary>
        private void OnSendMessageCommandExecuted(string p) => _Client.SendMessage(p);

        #endregion

        #endregion

        public MainWindowViewModel(ITCPClient Client)
        {
            _Client = Client;
            _Client.ReceiveMessage += OnMessageReceived;

            #region Команды

            ConnectCommand = new LambdaCommand(OnConnectCommandExecuted, CanConnectCommandExecute);
            DisconnectCommand = new LambdaCommand(OnDisconnectCommandExecuted, CanDisconnectCommandExecute);

            #endregion
        }

        private async void OnMessageReceived(object? Sender, EventArgs<string> E)
        {
            await Application.Current.Dispatcher;
            Messages.Add(new IncomingMessage { Message = E.Argument });
        }
    }
}
