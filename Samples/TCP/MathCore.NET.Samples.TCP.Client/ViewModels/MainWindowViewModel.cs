using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
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
        private string _Host;

        /// <summary>Адрес хоста</summary>
        public string Host { get => _Host; set => Set(ref _Host, value); }

        #endregion

        #region Port : int - Номер порта

        /// <summary>Номер порта</summary>
        private int _Port;

        /// <summary>Номер порта</summary>
        public int Port { get => _Port; set => Set(ref _Port, value); }

        #endregion

        #region Команды

        #region Command ConnectCommand - Подключиться к хосту

        /// <summary>Подключиться к хосту</summary>
        public ICommand ConnectCommand { get; }

        /// <summary>Проверка возможности выполнения - Подключиться к хосту</summary>
        private bool CanConnectCommandExecute() => true;

        /// <summary>Логика выполнения - Подключиться к хосту</summary>
        private void OnConnectCommandExecuted()
        {
            
        }

        #endregion

        #region Command DisconnectCommand - Отключиться от хоста

        /// <summary>Отключиться от хоста</summary>
        public ICommand DisconnectCommand { get; }

        /// <summary>Проверка возможности выполнения - Отключиться от хоста</summary>
        private bool CanDisconnectCommandExecute() => true;

        /// <summary>Логика выполнения - Отключиться от хоста</summary>
        private void OnDisconnectCommandExecuted()
        {
            
        }

        #endregion

        #endregion

        public MainWindowViewModel(ITCPClient Client)
        {
            _Client = Client;

            #region Команды

            ConnectCommand = new LambdaCommand(OnConnectCommandExecuted, CanConnectCommandExecute);
            DisconnectCommand = new LambdaCommand(OnDisconnectCommandExecuted, CanDisconnectCommandExecute);

            #endregion

        }
    }
}
