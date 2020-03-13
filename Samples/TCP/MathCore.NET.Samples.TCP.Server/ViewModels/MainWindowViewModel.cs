using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.WPF;
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

        #region Clients : ThreadSaveObservableCollectionWrapper<ClientViewModel> - Подключённые клиенты

        /// <summary>Подключённые клиенты</summary>
        private ThreadSaveObservableCollectionWrapper<ClientViewModel> _Clients = new ObservableCollection<ClientViewModel>().AsThreadSave();

        /// <summary>Подключённые клиенты</summary>
        public ThreadSaveObservableCollectionWrapper<ClientViewModel> Clients { get => _Clients; private set => Set(ref _Clients, value); }

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
            InitializeServer(_Server = Server);

            #region Команды

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanStartCommandExecute);
            StopCommand = new LambdaCommand(OnStopCommandExecuted, CanStopCommandExecute);

            #endregion
        }

        private void InitializeServer(ITCPServer Server)
        {
            if (!(Server.Clients is INotifyCollectionChanged collection)) return;
            collection.CollectionChanged += OnClientsCollectionChanged;
        }

        private void OnClientsCollectionChanged(object Sender, NotifyCollectionChangedEventArgs E)
        {
            switch (E.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var client in E.NewItems)
                    {
                        var client_view_model = new ClientViewModel((ITCPClient)client);
                        _Clients.Add(client_view_model);
                        Debug.WriteLine($"Connected:{client_view_model.Client.Host}({client_view_model.Client.Port})");
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var client in E.OldItems)
                    {
                        var old_client = _Clients.First(c => ReferenceEquals(c.Client, client));
                        if (old_client is null) continue;
                        old_client.Dispose();
                        _Clients.Remove(old_client);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var client in E.OldItems)
                    {
                        var old_client = _Clients.FirstOrDefault(c => ReferenceEquals(c.Client, client));
                        if (old_client is null) continue;
                        old_client.Dispose();
                        _Clients.Remove(old_client);
                    }

                    foreach (var client in E.NewItems)
                    {
                        var client_view_model = new ClientViewModel((ITCPClient)client);
                        _Clients.Add(client_view_model);
                        Debug.WriteLine($"Connected:{client_view_model.Client.Host}({client_view_model.Client.Port})");
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _Clients.Foreach(c => c.Dispose());
                    Clients = new ObservableCollection<ClientViewModel>(((IEnumerable<ITCPClient>)Sender).Select(c => new ClientViewModel(c))).AsThreadSave();
                    break;
            }
        }
    }
}
