namespace SqlServer.EtwWatcher
{
    /// <summary>
    /// Configuration class representing Shared Key and Customer Id to send Extended Events data to Azure Log Analytics Resource
    /// </summary>
    public class EtwWatcherAzureConfiguration
    {
        public string LogAnalyticsSharedKey { get; set; }

        public string LogAnalyticsCustomerId { get; set; }
    }
}