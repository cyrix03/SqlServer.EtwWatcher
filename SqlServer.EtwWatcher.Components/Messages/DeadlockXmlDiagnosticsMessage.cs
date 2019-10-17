using System;

namespace SqlServer.EtwWatcher.Components.Messages
{
    /// <summary>
    /// Diagnostics Message representing Deadlock Extended Event
    /// </summary>
    public class DeadlockXmlDiagnosticsMessage
    {
        public DateTime TimeGenerated { get; set; }

        public string BlockType { get; set; }

        public string Server { get; set; }

        public string Process1_Id { get; set; }

        public DateTime Process1_EventTime { get; set; }

        public string Process1_IsVictim { get; set; }

        public string Process1_Database { get; set; }

        public string Process1_Query { get; set; }

        public string Process1_LockMode { get; set; }

        public string Process1_ClientApp { get; set; }

        public string Process1_HostName { get; set; }

        public string Process1_LoginName { get; set; }

        public string Process1_Spid { get; set; }

        public string Process1_Table { get; set; }

        public string Process1_Index { get; set; }

        public string Process2_Id { get; set; }

        public DateTime Process2_EventTime { get; set; }

        public string Process2_IsVictim { get; set; }

        public string Process2_Database { get; set; }

        public string Process2_Query { get; set; }

        public string Process2_LockMode { get; set; }

        public string Process2_ClientApp { get; set; }

        public string Process2_HostName { get; set; }

        public string Process2_LoginName { get; set; }

        public string Process2_Spid { get; set; }

        public string Process2_Table { get; set; }

        public string Process2_Index { get; set; }
    }
}
