namespace CombatlogParser.Data
{
    /// <summary>
    /// Represents an entire Combatlog file.
    /// </summary>
    [Obsolete("Outdated type from the first early prototype. keeping for reference, but should not be used anymore.")]
    public class Combatlog
    {

        //These values can be found at the start of the file.
        public DateTime LogStartTimestamp { get; set; }
        public int CombatlogVersion { get; set; } = 19;
        public bool AdvancedLogEnabled { get; set; } = false;
        public string BuildVersion { get; set; } = "9.2.7";
        public int ProjectID { get; set; } = 1;

        /// <summary>
        /// All encounters in the log.
        /// </summary>
        public EncounterInfo[] Encounters { get; set; } = Array.Empty<EncounterInfo>();

        //TODO: zone_changed events should be handled, so we know which instance this is in.
        public LogEntryBase[] ZoneChangeEvents = Array.Empty<LogEntryBase>();
    }
}
