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

        public string SourceGUID { get; set; } = "None-0000-00000000";
        public string SourceName { get; set; } = "Unnamed";
        public UnitFlag SourceFlags { get; set; } = 0x0;
        public RaidFlag SourceRaidFlags { get; set; } = 0x0;
        public string TargetGUID { get; set; } = "None-0000-00000000";
        public string TargetName { get; set; } = "Unnamed";
        public UnitFlag TargetFlags { get; set; } = 0x0;
        public RaidFlag TargetRaidFlags { get; set; } = 0x0;

        public bool IsSourcePet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

        /// <summary>
        /// The parameters specific to the Subevents Prefix
        /// </summary>
        public object[] PrefixParams { get; set; } = Array.Empty<object>();

        public object PrefixParam0 => PrefixParams.Length >= 1 ? PrefixParams[0] : "";
        public object PrefixParam1 => PrefixParams.Length >= 2 ? PrefixParams[1] : "";
        public object PrefixParam2 => PrefixParams.Length >= 3 ? PrefixParams[2] : "";

        /// <summary>
        /// The parameters specific to the Subevents Suffix
        /// </summary>
        public object[] SuffixParams { get; set; } = Array.Empty<object>();

        public object SuffixParam0 => SuffixParams.Length >= 1 ? SuffixParams[0] : "";
        public object SuffixParam1 => SuffixParams.Length >= 2 ? SuffixParams[1] : "";
        public object SuffixParam2 => SuffixParams.Length >= 3 ? SuffixParams[2] : "";
        public object SuffixParam3 => SuffixParams.Length >= 4 ? SuffixParams[3] : "";
        public object SuffixParam4 => SuffixParams.Length >= 5 ? SuffixParams[4] : "";
        public object SuffixParam5 => SuffixParams.Length >= 6 ? SuffixParams[5] : "";
        public object SuffixParam6 => SuffixParams.Length >= 7 ? SuffixParams[6] : "";
        public object SuffixParam7 => SuffixParams.Length >= 8 ? SuffixParams[7] : "";
        public object SuffixParam8 => SuffixParams.Length >= 9 ? SuffixParams[8] : "";
        public object SuffixParam9 => SuffixParams.Length >= 10 ? SuffixParams[9] : "";
        public object SuffixParam10 => SuffixParams.Length >= 11 ? SuffixParams[10] : "";

        /// <summary>
        /// The advanced combatlog parameters.
        /// </summary>
        public string[] AdvancedParams { get; set; } = Array.Empty<string>();

    }
}
