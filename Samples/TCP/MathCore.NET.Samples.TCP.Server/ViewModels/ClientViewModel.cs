using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.WPF.ViewModels;

namespace MathCore.NET.Samples.TCP.Server.ViewModels
{
    class ClientViewModel : ViewModel
    {
        private ITCPClient _Client;

        public ITCPClient Client => _Client;

        public ClientViewModel(ITCPClient Client) => InitializeClient(_Client = Client);

        private void InitializeClient(ITCPClient client)
        {

        }

        private void FinalizeClient(ITCPClient client)
        {
            if (Client is null) return;

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            FinalizeClient(_Client);
            _Client = null;
        }
    }
}
