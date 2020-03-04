using System.Windows;

namespace MathCore.NET.Samples.TCP.Server
{
    public partial class App
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await Program.AppHost.StartAsync().ConfigureAwait(false);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            await Program.AppHost.StopAsync().ConfigureAwait(false);
        }
    }
}
