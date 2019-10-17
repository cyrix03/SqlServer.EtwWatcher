namespace SqlServer.EtwWatcher.Components.Messages
{
    /// <summary>
    /// Representation of the different types of Diagnostics Message types that could be generated from Sql Server Extended Events
    /// </summary>
    public static class DiagnosticsMessageType
    {
        public const string DeadlockXmlMessage = "Deadlock";
    }
}
