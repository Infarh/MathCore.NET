using System;

using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.ViewModels;

namespace MathCore.NET.Samples.TCP.Server.ViewModels
{
    class IncomingMessage : ViewModel
    {
        #region Time : DateTime - Время

        /// <summary>Время</summary>
        private DateTime _Time = DateTime.Now;

        /// <summary>Время</summary>
        public DateTime Time { get => _Time; set => Set(ref _Time, value); }

        #endregion

        #region Message : string - Сообщение

        /// <summary>Сообщение</summary>
        private string _Message;

        /// <summary>Сообщение</summary>
        public string Message { get => _Message; set => Set(ref _Message, value); }

        #endregion

        public ITCPClient Client { get; set; }
    }
}
