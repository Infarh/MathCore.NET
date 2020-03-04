using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathCore.NET.Samples.TCP.Client
{
    public static class Program
    {
        public static IHost AppHost { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            using (AppHost = CreateHostBuilder(args).Build())
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .UseContentRoot(Environment.CurrentDirectory)
           .ConfigureAppConfiguration((host, config) => config
               .SetBasePath(host.HostingEnvironment.ContentRootPath)
               .AddJsonFile("appsettings.json"))
           .ConfigureServices(ConfigureServices)
        ;

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection Services)
        {
            var configuration = context.Configuration;
        }
    }
}
