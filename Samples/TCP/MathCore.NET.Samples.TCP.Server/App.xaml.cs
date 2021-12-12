using System;
using System.Windows;
using MathCore.NET.Samples.TCP.Server.Services;
using MathCore.NET.Samples.TCP.Server.Services.Interfaces;
using MathCore.NET.Samples.TCP.Server.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathCore.NET.Samples.TCP.Server
{
    public partial class App
    {
        public static IHost Host { get; } = Program
           .CreateHostBuilder(Environment.GetCommandLineArgs())
           .Build();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await Host.StartAsync().ConfigureAwait(false);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            await Host.StopAsync().ConfigureAwait(false);
            Host.Dispose();
        }

        public static void ConfigureServices(HostBuilderContext context, IServiceCollection Services)
        {
            var configuration = context.Configuration;

            Services.AddSingleton<MainWindowViewModel>();

            Services.AddSingleton<ITCPServer, TCPServer>();
        }
    }
}
