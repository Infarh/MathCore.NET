using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
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

        public MainWindowViewModel(ITCPServer Server)
        {
            _Server = Server;
        }
    }
}
