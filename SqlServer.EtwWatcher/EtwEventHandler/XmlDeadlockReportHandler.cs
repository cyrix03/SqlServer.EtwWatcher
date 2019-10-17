using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using SqlServer.EtwWatcher.Components.Messages;
using Microsoft.SqlServer.XEvent.XELite;

using Newtonsoft.Json;
using Serilog;

namespace SqlServer.EtwWatcher
{
    /// <summary>
    /// <see cref="EtwEventHandler.Handle(IXEvent, EtwWatcherInstanceConfiguration, EtwWatcherAzureConfiguration)"/> Implentation for Deadlock events created by <see cref="XELiveEventStreamer"/>
    /// </summary>
    public class XmlDeadlockReportHandler :
        EtwEventHandler
    {

        readonly ILogger logger;

        //readonly LogAnalyticsSink logAnalyticsSink;

        public XmlDeadlockReportHandler(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<bool> Handle(IXEvent xEvent, EtwWatcherInstanceConfiguration instanceConfig, EtwWatcherAzureConfiguration azureConfig)
        {
            if (xEvent.Name != "xml_deadlock_report")
                return false;

            //Get Current time for Azure Signature
            var utcTime = DateTime.UtcNow;

            //Load Report and Server objects from xEvent
            xEvent.Fields.TryGetValue("xml_report", out object report);

            //Generate Deadlock object from xml_report
            var deadlock = XDocument
                .Parse(Regex.Replace(report.ToString().Trim(), @"[^\u0000-\u007F]+", string.Empty))
                .Elements("deadlock")
                .Select(a => new
                {
                    Victim = a.Elements("victim-list")
                        .Elements("victimProcess")
                        .Select(i => (string)i.Attribute("id")).ToList(),
                    ProcessList = a.Elements("process-list")
                        .Elements("process")
                        .Select(async i => new
                        {
                            id = (string)i.Attribute("id"),
                            taskpriority = (string)i.Attribute("taskpriority"),
                            logused = (string)i.Attribute("logused"),
                            waitresource = (string)i.Attribute("waitresource"),
                            waittime = (string)i.Attribute("waittime"),
                            ownerId = (string)i.Attribute("ownerId"),
                            transactionname = (string)i.Attribute("transactionname"),
                            lasttranstarted = ((DateTime)i.Attribute("lasttranstarted")).ToUniversalTime(),
                            XDES = (string)i.Attribute("XDES"),
                            lockMode = (string)i.Attribute("lockMode"),
                            schedulerid = (string)i.Attribute("schedulerid"),
                            kpid = (string)i.Attribute("kpid"),
                            status = (string)i.Attribute("status"),
                            spid = (string)i.Attribute("spid"),
                            sbid = (string)i.Attribute("sbid"),
                            ecid = (string)i.Attribute("ecid"),
                            priority = (string)i.Attribute("priority"),
                            trancount = (string)i.Attribute("trancount"),
                            lastbatchstarted = ((DateTime)i.Attribute("lastbatchstarted")).ToUniversalTime(),
                            lastbatchcompleted = ((DateTime)i.Attribute("lastbatchcompleted")).ToUniversalTime(),
                            lastattention = ((DateTime)i.Attribute("lastattention")).ToUniversalTime(),
                            clientapp = (string)i.Attribute("clientapp"),
                            hostname = (string)i.Attribute("hostname"),
                            hostpid = (string)i.Attribute("hostpid"),
                            loginname = (string)i.Attribute("loginname"),
                            isolationlevel = (string)i.Attribute("isolationlevel"),
                            xactid = (string)i.Attribute("xactid"),
                            currentdb = (int)i.Attribute("currentdb"),
                            currentdbName = await instanceConfig.DatabaseCache.GetOrAdd((int)i.Attribute("currentdb"), AddValue((int)i.Attribute("currentdb"), instanceConfig.ConnectionString)),
                            lockTimeout = (string)i.Attribute("lockTimeout"),
                            clientoption1 = (string)i.Attribute("clientoption1"),
                            clientoption2 = (string)i.Attribute("clientoption2"),
                            executionStack = i.Elements("executionStack").Elements("frame").Select(j => new
                            {
                                procname = (string)j.Attribute("procname"),
                                line = (string)j.Attribute("line"),
                                stmtstart = (string)j.Attribute("stmtstart"),
                                stmtend = (string)j.Attribute("stmtend"),
                                sqlhandle = (string)j.Attribute("sqlhandle"),
                            }).ToList(),
                            inputbuf = ((string)i.Element("inputbuf")).Trim(),
                        })
                        .Select(t => t.Result)
                        .ToList(),
                    ResourceList = a.Elements("resource-list")
                        .Elements("keylock")
                        .Select(i => new
                        {
                            hobtid = (string)i.Attribute("hobtid"),
                            dbid = (string)i.Attribute("dbid"),
                            objectname = (string)i.Attribute("objectname"),
                            indexname = (string)i.Attribute("indexname"),
                            id = (string)i.Attribute("id"),
                            mode = (string)i.Attribute("mode"),
                            associatedObjectId = (string)i.Attribute("associatedObjectId"),
                            ownerLlist = i.Elements("owner-list").Elements("owner").Select(j => new
                            {
                                id = (string)j.Attribute("id"),
                                mode = (string)j.Attribute("mode")
                            }).ToList(),
                            waiterList = i.Elements("waiter").Select(j => new
                            {
                                id = (string)j.Attribute("id"),
                                mode = (string)j.Attribute("mode"),
                                requestType = (string)j.Attribute("requestType")
                            }).ToList()
                        }).ToList()
                }).First();

            //New Object List to Message
            var deadlockXmlDiagnosticsMessage = new DeadlockXmlDiagnosticsMessage
            {
                TimeGenerated = xEvent.Timestamp.UtcDateTime,
                BlockType = DiagnosticsMessageType.DeadlockXmlMessage,
                Server = Environment.MachineName,
                Process1_Id = deadlock.ProcessList[0].id,
                Process1_EventTime = deadlock.ProcessList[0].lasttranstarted,
                Process1_IsVictim = deadlock.Victim.First() == deadlock.ProcessList[0].id ? "True" : "False",
                Process1_LockMode = deadlock.ProcessList[0].lockMode,
                Process1_Spid = deadlock.ProcessList[0].spid,
                Process1_ClientApp = deadlock.ProcessList[0].clientapp,
                Process1_HostName = deadlock.ProcessList[0].hostname,
                Process1_LoginName = deadlock.ProcessList[0].loginname,
                Process1_Database = deadlock.ProcessList[0].currentdbName,
                Process1_Query = deadlock.ProcessList[0].inputbuf.Trim(),
                Process1_Table = deadlock.ResourceList.Where(i => i.ownerLlist[0].id == deadlock.ProcessList[0].id).Select(a => a.objectname).First(),
                Process1_Index = deadlock.ResourceList.Where(i => i.ownerLlist[0].id == deadlock.ProcessList[0].id).Select(a => a.indexname).First(),
                Process2_Id = deadlock.ProcessList[1].id,
                Process2_EventTime = deadlock.ProcessList[1].lasttranstarted,
                Process2_IsVictim = deadlock.Victim.First() == deadlock.ProcessList[1].id ? "True" : "False",
                Process2_LockMode = deadlock.ProcessList[1].lockMode,
                Process2_Spid = deadlock.ProcessList[1].spid,
                Process2_ClientApp = deadlock.ProcessList[1].clientapp,
                Process2_HostName = deadlock.ProcessList[1].hostname,
                Process2_LoginName = deadlock.ProcessList[1].loginname,
                Process2_Database = deadlock.ProcessList[1].currentdbName,
                Process2_Query = deadlock.ProcessList[1].inputbuf.Trim(),
                Process2_Table = deadlock.ResourceList.Where(i => i.ownerLlist[0].id == deadlock.ProcessList[1].id).Select(a => a.objectname).First(),
                Process2_Index = deadlock.ResourceList.Where(i => i.ownerLlist[0].id == deadlock.ProcessList[1].id).Select(a => a.indexname).First()
            };

            //await logAnalyticsSink.Send(deadlockXmlDiagnosticsMessage);

            var signature = AzureUtil.BuildSignature(JsonConvert.SerializeObject(deadlockXmlDiagnosticsMessage), azureConfig.LogAnalyticsSharedKey, Guid.Parse(azureConfig.LogAnalyticsCustomerId), utcTime.ToString("r"));

            logger.Information("Result from Azure: " + await AzureUtil.PostData(signature, utcTime.ToString("r"), JsonConvert.SerializeObject(deadlockXmlDiagnosticsMessage), Guid.Parse(azureConfig.LogAnalyticsCustomerId)));

            return true;
        }

        /// <summary>
        /// Add Databases to <see cref="EtwWatcherInstanceConfiguration.DatabaseCache"/>.
        /// </summary>
        /// <param name="dbId"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private async Task<string> AddValue(int dbId, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT DB_NAME(" + dbId.ToString() + ")";
                using (var cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    var returnValue = (string)await cmd.ExecuteScalarAsync();
                    connection.Close();
                    return returnValue;
                }
            }
        }
    }
}
