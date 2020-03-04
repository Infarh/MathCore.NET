using System;
using System.Collections.Generic;
using System.Text;
using MathCore.WPF.ViewModels;

namespace MathCore.NET.Samples.TCP.Client.ViewModels
{
    class MainWindowViewModel : ViewModel
    {
        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Клиент";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion
    }
}
