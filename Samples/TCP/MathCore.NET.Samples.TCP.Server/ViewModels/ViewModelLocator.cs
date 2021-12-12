using System;
using Microsoft.Extensions.DependencyInjection;

namespace MathCore.NET.Samples.TCP.Server.ViewModels
{
    class ViewModelLocator
    {
        private readonly IServiceProvider _Services;

        public MainWindowViewModel MainWindowModel => _Services.GetRequiredService<MainWindowViewModel>();

        public ViewModelLocator() => _Services = (App.Host ?? Program.CreateHostBuilder(Array.Empty<string>()).Build()).Services;
    }
}
