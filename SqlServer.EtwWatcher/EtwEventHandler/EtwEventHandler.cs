using Microsoft.SqlServer.XEvent.XELite;
using System.Threading.Tasks;

namespace SqlServer.EtwWatcher
{
    /// <summary>
    /// Abstract class that Handles <see cref="XELiveEventStreamer"/>s
    /// </summary>
    public abstract class EtwEventHandler
    {
        abstract public Task<bool> Handle(IXEvent xEvent, EtwWatcherInstanceConfiguration instanceConfig, EtwWatcherAzureConfiguration azureConfig);

    }
}
