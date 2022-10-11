namespace CombatlogParser.Data
{
    /// <summary>
    /// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
    /// This is a somewhat polymorphic class.
    /// </summary>
    public class CombatlogEvent : LogEntryBase
    {
        //the first 10 parameters always exist - for combat events. other events may differ.
        public CombatlogEventPrefix SubeventPrefix { get; set; } = CombatlogEventPrefix.UNDEFINED;
        public CombatlogEventSuffix SubeventSuffix { get; set; } = CombatlogEventSuffix.UNDEFINED;

        public string SubEvent { get => SubeventPrefix.ToString() + SubeventSuffix.ToString(); }

        public string SourceUID { get; set; } = "None-0000-00000000";
        public string SourceName { get; set; } = "Unnamed";
        public int SourceFlags { get; set; } = 0x0;
        public int SourceRaidFlags { get; set; } = 0x0;
        public string TargetUID { get; set; } = "None-0000-00000000";
        public string TargetName { get; set; } = "Unnamed";
        public int TargetFlags { get; set; } = 0x0;
        public int TargetRaidFlags { get; set; } = 0x0;

        /// <summary>
        /// The parameters specific to the Subevents Prefix
        /// </summary>
        public object[] PrefixParams { get; set; } = Array.Empty<object>();

        /// <summary>
        /// The parameters specific to the Subevents Suffix
        /// </summary>
        public object[] SuffixParams { get; set; } = Array.Empty<object>();

        /// <summary>
        /// The advanced combatlog parameters.
        /// </summary>
        public string[] AdvancedParams { get; set; } = Array.Empty<string>();

    }
}
