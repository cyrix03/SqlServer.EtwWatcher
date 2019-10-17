using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlServer.EtwWatcher.Components.Messages;
using System.Threading.Tasks;
using NUnit.Framework;
using SqlServer.EtwWatcher.Tests.Data;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Linq;

namespace SqlServer.EtwWatcher.Tests
{
    [TestFixture]
    public class XEventTest
    {
        /// <summary>
        /// Get simulated Deadlock XEvent
        /// </summary>
        public DeadLockXEventData DeadLockXEvent { get; set; } = new DeadLockXEventData();

        /// <summary>
        /// Get Simulated Azure CustomerId
        /// </summary>
        public Guid CustomerId { get; set; } = Guid.Parse("5a57b610-6006-4ed5-848e-2a8324262c28");

        /// <summary>
        /// Get Simulated Azure Shared Key
        /// </summary>
        public string SharedKey { get; set; } = "tH1s1z4RaNd0m5tR1g70s1muL473AzUr3Sh4r3dK3yASdf7u908123sdfSDFaDBkN2Wa4RVna7H123hSDfyySw==";

        /// <summary>
        /// Assert Simulated <see cref="Microsoft.SqlServer.XEvent.XELite.IXEvent"/> to <see cref="SqlServer.EtwWatcher.Components.Messages.DeadlockXmlDiagnosticsMessage"/> matches hard coded test
        /// </summary>
        [Test]
        public void Assert_Xml_Deadlock_Report()
        {
            CreateDeadlockXmlDiagnosticsMessageFromXEvent().Should().BeEquivalentTo(CreateDeadlockXmlDiagnosticsMessageFromJson());
        }

        /// <summary>
        /// Test <see cref="AzureUtil.BuildSignature(string, string, Guid, string)"/> generates correctly
        /// </summary>
        [Test]
        public void Assert_Xml_Deadlock_Report_BuildSignature()
        {
            var time = DateTime.Parse("2019-01-01 00:00:00").ToUniversalTime();
            var message = JsonConvert.SerializeObject(CreateDeadlockXmlDiagnosticsMessageFromJson());
            var buildSignature = AzureUtil.BuildSignature(message.ToString(), SharedKey, CustomerId, time.ToString("r"));

            buildSignature.Should().Be("SharedKey 5a57b610-6006-4ed5-848e-2a8324262c28:VngkZ+DskKUMSHVEMfTP2lraIWxllcjl2TsrND9PynU=");
        }

        /// <summary>
        /// Negative Test for <see cref="AzureUtil.PostData(string, string, string, Guid)"/>
        /// </summary>
        [Test]
        public async Task Assert_Xml_Deadlock_Report_PostData_Fails()
        {
            var time = DateTime.UtcNow;
            var message = JsonConvert.SerializeObject(CreateDeadlockXmlDiagnosticsMessageFromJson());

            var buildSignature = AzureUtil.BuildSignature(message.ToString(), SharedKey, CustomerId, time.ToString("r"));

            var result = await AzureUtil.PostData(buildSignature, time.ToString("r"), message, CustomerId);

            result.Should().Be("API Post Exception : No such host is known.");
        }

        /// <summary>
        /// HardCoded <see cref="DeadlockXmlDiagnosticsMessage"/>
        /// </summary>
        /// <returns><see cref="DeadlockXmlDiagnosticsMessage"/></returns>
        public DeadlockXmlDiagnosticsMessage CreateDeadlockXmlDiagnosticsMessageFromJson() =>
            JObject.Parse(@"
            {
                ""TimeGenerated"": ""2019-07-29 18:56:28.5065663"",
                ""BlockType"": ""Deadlock"",
                ""Server"": """ + Environment.MachineName + @""",
                ""Process1_Id"": ""process269fd14e108"",
                ""Process1_EventTime"": ""2019-10-16 21:12:40.380"",
                ""Process1_IsVictim"": ""True"",
                ""Process1_Database"": ""TestDatabase"",
                ""Process1_Query"": ""BEGIN TRAN INSERT dbo.Tbl2 (id, col) VALUES (111, 2)"",
                ""Process1_LockMode"": ""S"",
                ""Process1_ClientApp"": ""Microsoft SQL Server Management Studio - Query"",
                ""Process1_HostName"": ""TestHostName"",
                ""Process1_LoginName"": ""TestUser"",
                ""Process1_Spid"" : ""52"",
                ""Process1_Table"" : ""TestDatabase.dbo.Tbl2"",
                ""Process1_Index"" : ""PK__Tbl2__3213E83F1AB5FCC0"",
                ""Process2_Id"": ""process26a3202f848"",
                ""Process2_EventTime"": ""2019-10-16 21:12:37.737"",
                ""Process2_IsVictim"": ""False"",
                ""Process2_Database"": ""TestDatabase"",
                ""Process2_Query"": ""INSERT dbo.Tbl2 (id, col) VALUES (111, 555)"",
                ""Process2_LockMode"": ""X"",
                ""Process2_ClientApp"": ""Microsoft SQL Server Management Studio - Query"",
                ""Process2_HostName"": ""TestHostName"",
                ""Process2_LoginName"": ""TestUser"",
                ""Process2_Spid"" : ""53"",
                ""Process2_Table"" : ""TestDatabase.dbo.Tbl1"",
                ""Process2_Index"" : ""PK__Tbl1__3213E83F8A1B29B7""
            }").ToObject<DeadlockXmlDiagnosticsMessage>();

        /// <summary>
        /// Generate <see cref="DeadlockXmlDiagnosticsMessage"/> from Simulated <see cref="DeadLockXEventData"/>
        /// </summary>
        /// <returns><see cref="DeadlockXmlDiagnosticsMessage"/></returns>
        public DeadlockXmlDiagnosticsMessage CreateDeadlockXmlDiagnosticsMessageFromXEvent()
        {
            DeadLockXEvent.Fields.TryGetValue("xml_report", out object report);

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
                            currentdbName = "TestDatabase",
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

            return new DeadlockXmlDiagnosticsMessage
            {
                TimeGenerated = DeadLockXEvent.Timestamp.UtcDateTime,
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
        }
    }
}