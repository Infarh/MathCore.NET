using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
// ReSharper disable ArgumentsStyleLiteral

namespace MathCore.NET.Samples.TCP.Client
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .UseContentRoot(Environment.CurrentDirectory)
           .ConfigureAppConfiguration((host, config) => config
               .SetBasePath(host.HostingEnvironment.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true))
           .ConfigureServices(App.ConfigureServices)
        ;
    }
}
