using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SqlServer.EtwWatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Information("Creating Host {DateTime}", DateTimeOffset.Now);
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "There was a problem starting the service...");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Create Generic Host to run service
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    //Load Config Files
                    config.AddJsonFile("app.config.json", true, true);
                })
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    //Register Logger
                    builder.RegisterInstance(
                        new LoggerConfiguration()
#if DEBUG
                            .WriteTo.ColoredConsole()
#else
                            .WriteTo.EventLog("SqlServer.EtwWatcher Service", manageEventSource: true)
#endif
                            .CreateLogger()).As<ILogger>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("app.config.json", false, true)
                        .Build();

                    services.AddOptions();
                    services.Configure<EtwWatcherAzureConfiguration>(configuration.GetSection(typeof(EtwWatcherAzureConfiguration).Name));
                    services.Configure<EtwWatcherInstanceConfiguration>(configuration.GetSection(typeof(EtwWatcherInstanceConfiguration).Name));
                    services.AddSingleton<EtwWatcherServiceConfiguration>();

                    services.TryAddScoped<EtwEventHandler, XmlDeadlockReportHandler>();

                    services.AddHostedService<EtwWatcherService>();
                })
                .UseSerilog();
    }
}
