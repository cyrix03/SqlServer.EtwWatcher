using Microsoft.Extensions.Options;
using System;

namespace SqlServer.EtwWatcher
{
    /// <summary>
    /// Configuration class that represnets Azure Configuration to send logs too and Instance configuration for what Sql Server instance to listening for <see cref="XELiveEventStreamer"/>s
    /// </summary>
    public class EtwWatcherServiceConfiguration 
    {
        readonly IOptions<EtwWatcherAzureConfiguration> etwWatcherAzureConfiguration;
        readonly IOptions<EtwWatcherInstanceConfiguration> etwWatcherInstanceConfiguration;

        public EtwWatcherServiceConfiguration(IOptions<EtwWatcherAzureConfiguration> etwWatcherAzureConfiguration, IOptions<EtwWatcherInstanceConfiguration> etwWatcherInstanceConfiguration)
        {
            this.etwWatcherAzureConfiguration = etwWatcherAzureConfiguration ?? throw new ArgumentNullException(nameof(etwWatcherAzureConfiguration));
            this.etwWatcherInstanceConfiguration = etwWatcherInstanceConfiguration ?? throw new ArgumentNullException(nameof(etwWatcherInstanceConfiguration));
            AzureConfiguration = etwWatcherAzureConfiguration.Value;
            Instances = etwWatcherInstanceConfiguration.Value;
        }

        public EtwWatcherAzureConfiguration AzureConfiguration { get; set; }
        public EtwWatcherInstanceConfiguration Instances { get; set; }

    }
}

