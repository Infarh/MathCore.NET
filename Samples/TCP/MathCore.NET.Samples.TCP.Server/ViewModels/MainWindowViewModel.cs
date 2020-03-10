using System.Windows.Input;
using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;

namespace MathCore.NET.Samples.TCP.Server.ViewModels
{
    class MainWindowViewModel : ViewModel
    {
        private readonly ITCPServer _Server;

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Сервер";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        #region Port : int - Порт сервера

        /// <summary>Порт сервера</summary>
        private int _Port = 8080;

        /// <summary>Порт сервера</summary>
        public int Port { get => _Port; set => Set(ref _Port, value); }

        #endregion

        #region Команды

        #region Command StartCommand - Запуск сервера

        /// <summary>Запуск сервера</summary>
        public ICommand StartCommand { get; }

        /// <summary>Проверка возможности выполнения - Запуск сервера</summary>
        private bool CanStartCommandExecute() => !_Server.Enabled;

        /// <summary>Логика выполнения - Запуск сервера</summary>
        private void OnStartCommandExecuted() => _Server.Start(_Port);

        #endregion

        #region Command StopCommand - Остановка сервера

        /// <summary>Остановка сервера</summary>
        public ICommand StopCommand { get; }

        /// <summary>Проверка возможности выполнения - Остановка сервера</summary>
        private bool CanStopCommandExecute() => _Server.Enabled;

        /// <summary>Логика выполнения - Остановка сервера</summary>
        private void OnStopCommandExecuted() => _Server.Stop();

        #endregion

        #endregion

        public MainWindowViewModel(ITCPServer Server)
        {
            _Server = Server;

            #region Команды

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanStartCommandExecute);
            StopCommand = new LambdaCommand(OnStopCommandExecuted, CanStopCommandExecute);

            #endregion
        }
    }
}
