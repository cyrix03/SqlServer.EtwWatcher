using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.XEvent.XELite;

using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.EtwWatcher
{
    /// <summary>
    /// Event Tracing for Windows (ETW) Service. Listens for <see cref="XELiveEventStreamer"/>s and Handles them by implementing <see cref="EtwEventHandler.Handle(IXEvent, EtwWatcherInstanceConfiguration, EtwWatcherAzureConfiguration)"/>
    /// </summary>
    public class EtwWatcherService :
        BackgroundService
    {
        readonly ILogger logger;
        readonly EtwWatcherServiceConfiguration config;
        readonly IEnumerable<EtwEventHandler> handlers;

        public EtwWatcherService(ILogger logger, EtwWatcherServiceConfiguration config, IEnumerable<EtwEventHandler> handlers)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.handlers = handlers?.ToList() ?? throw new ArgumentNullException(nameof(handlers));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Information("Starting EtwWatcher Service {DateTime}", DateTimeOffset.Now);
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Information("Stopping Service {DateTime}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //foreach (var item in config.Instances)
                //{
                    try
                    {
                        var xeStream = new XELiveEventStreamer(config.Instances.ConnectionString, config.Instances.SessionName);

                        await xeStream.ReadEventStream(xEvent => HandleXEventFuncAsync(xEvent, config.Instances, config.AzureConfiguration), stoppingToken);

                        Console.ReadLine();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, e.Message);
                    }
               //}
            }
        }

        private async Task HandleXEventFuncAsync(IXEvent xEvent, EtwWatcherInstanceConfiguration instanceConfig, EtwWatcherAzureConfiguration azureConfig)
        {
            foreach (var handler in handlers)
                if(await handler.Handle(xEvent, instanceConfig, azureConfig))
                    return;
        }
    }
}
