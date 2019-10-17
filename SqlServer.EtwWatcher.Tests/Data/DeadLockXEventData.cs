using Microsoft.SqlServer.XEvent.XELite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SqlServer.EtwWatcher.Tests.Data
{
    /// <summary>
    /// Testing class for Deadlock <see cref="Microsoft.SqlServer.XEvent.XELite.IXEvent"/>s
    /// </summary>
    public class DeadLockXEventData
        : IXEvent
    {
        public string Name { get; set; } = "xml_deadlock_report";

        public Guid UUID { get; set; } = Guid.Parse("5a57b610-6006-4ed5-848e-2a8324262c28");

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Parse("2019-07-29T13:56:28.5065663-05:00");

        public IReadOnlyDictionary<string, object> Fields
        {
            get
            {
                var test = new Dictionary<string, object>
                {
                    { "xml_report" , "<deadlock>\r\n <victim-list>\r\n  <victimProcess id=\"process269fd14e108\"/>\r\n </victim-list>\r\n <process-list>\r\n  <process id=\"process269fd14e108\" taskpriority=\"0\" logused=\"292\" waitresource=\"KEY: 10:72057594095271936 (61a06abd401c)\" waittime=\"7714\" ownerId=\"1125615\" transactionname=\"user_transaction\" lasttranstarted=\"2019-10-16T16:12:40.380\" XDES=\"0x26a2ca54490\" lockMode=\"S\" schedulerid=\"4\" kpid=\"28324\" status=\"suspended\" spid=\"52\" sbid=\"0\" ecid=\"0\" priority=\"0\" trancount=\"2\" lastbatchstarted=\"2019-10-16T16:12:40.380\" lastbatchcompleted=\"2019-10-16T16:12:40.377\" lastattention=\"1900-01-01T00:00:00.377\" clientapp=\"Microsoft SQL Server Management Studio - Query\" hostname=\"TestHostName\" hostpid=\"19420\" loginname=\"TestUser\" isolationlevel=\"read committed (2)\" xactid=\"1125615\" currentdb=\"10\" lockTimeout=\"4294967295\" clientoption1=\"671090784\" clientoption2=\"390200\">\r\n   <executionStack>\r\n    <frame procname=\"adhoc\" line=\"2\" stmtstart=\"30\" stmtend=\"126\" sqlhandle=\"0x0200000009de4f2af1405ce009441becd4cbefef3e71f3290000000000000000000000000000000000000000\">\r\nunknown    </frame>\r\n    <frame procname=\"adhoc\" line=\"2\" stmtstart=\"24\" stmtend=\"104\" sqlhandle=\"0x020000008d2ab9030650ebc0eab587b94eebe42bfed9ea400000000000000000000000000000000000000000\">\r\nunknown    </frame>\r\n   </executionStack>\r\n   <inputbuf>BEGIN TRAN INSERT dbo.Tbl2 (id, col) VALUES (111, 2)</inputbuf>\r\n  </process>\r\n  <process id=\"process26a3202f848\" taskpriority=\"0\" logused=\"292\" waitresource=\"KEY: 10:72057594095337472 (c20918c6e169)\" waittime=\"4840\" ownerId=\"1125599\" transactionname=\"user_transaction\" lasttranstarted=\"2019-10-16T16:12:37.737\" XDES=\"0x26a08ae8490\" lockMode=\"X\" schedulerid=\"3\" kpid=\"35148\" status=\"suspended\" spid=\"53\" sbid=\"0\" ecid=\"0\" priority=\"0\" trancount=\"2\" lastbatchstarted=\"2019-10-16T16:12:43.253\" lastbatchcompleted=\"2019-10-16T16:12:43.250\" lastattention=\"2019-10-16T12:32:06.590\" clientapp=\"Microsoft SQL Server Management Studio - Query\" hostname=\"TestHostName\" hostpid=\"19420\" loginname=\"TestUser\" isolationlevel=\"read committed (2)\" xactid=\"1125599\" currentdb=\"10\" lockTimeout=\"4294967295\" clientoption1=\"671090784\" clientoption2=\"390200\">\r\n   <executionStack>\r\n    <frame procname=\"adhoc\" line=\"1\" stmtstart=\"30\" stmtend=\"126\" sqlhandle=\"0x0200000009de4f2af1405ce009441becd4cbefef3e71f3290000000000000000000000000000000000000000\">\r\nunknown    </frame>\r\n    <frame procname=\"adhoc\" line=\"1\" stmtend=\"84\" sqlhandle=\"0x02000000dcc60f0386a6487bf053a44870ed1152ebeb66130000000000000000000000000000000000000000\">\r\nunknown    </frame>\r\n   </executionStack>\r\n   <inputbuf>\r\nINSERT dbo.Tbl2 (id, col) VALUES (111, 555)\r\n   </inputbuf>\r\n  </process>\r\n </process-list>\r\n <resource-list>\r\n  <keylock hobtid=\"72057594095271936\" dbid=\"10\" objectname=\"TestDatabase.dbo.Tbl1\" indexname=\"PK__Tbl1__3213E83F8A1B29B7\" id=\"lock26a326df100\" mode=\"X\" associatedObjectId=\"72057594095271936\">\r\n   <owner-list>\r\n    <owner id=\"process26a3202f848\" mode=\"X\"/>\r\n   </owner-list>\r\n   <waiter-list>\r\n    <waiter id=\"process269fd14e108\" mode=\"S\" requestType=\"wait\"/>\r\n   </waiter-list>\r\n  </keylock>\r\n  <keylock hobtid=\"72057594095337472\" dbid=\"10\" objectname=\"TestDatabase.dbo.Tbl2\" indexname=\"PK__Tbl2__3213E83F1AB5FCC0\" id=\"lock26a04ee7d80\" mode=\"X\" associatedObjectId=\"72057594095337472\">\r\n   <owner-list>\r\n    <owner id=\"process269fd14e108\" mode=\"X\"/>\r\n   </owner-list>\r\n   <waiter-list>\r\n    <waiter id=\"process26a3202f848\" mode=\"X\" requestType=\"wait\"/>\r\n   </waiter-list>\r\n  </keylock>\r\n </resource-list>\r\n</deadlock>\r\n" }
                };

                return new ReadOnlyDictionary<string, object>(test);
            }
        }

        public IReadOnlyDictionary<string, object> Actions { get; set; }
    }
}