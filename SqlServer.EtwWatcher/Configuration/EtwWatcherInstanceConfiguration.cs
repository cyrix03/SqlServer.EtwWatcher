using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SqlServer.EtwWatcher
{
    /// <summary>
    /// Configuration Class to set up Connection String, Sql Server Extneded Events Session, and Dictionary representing the Database Id and Database Name
    /// </summary>
    public class EtwWatcherInstanceConfiguration
    {
        public string ConnectionString { get; set; }

        public string SessionName { get; set; }

        public ConcurrentDictionary<int, Task<string>> DatabaseCache { get; set; } = new ConcurrentDictionary<int, Task<string>>();

    }
}