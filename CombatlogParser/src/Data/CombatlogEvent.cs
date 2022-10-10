namespace CombatlogParser.Data
{
    /// <summary>
    /// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
    /// This is a somewhat polymorphic class.
    /// </summary>
    public class CombatlogEvent
    {
        //the first 10 parameters always exist.
        public string Timestamp { get; set; } = "00/00 00:00:00.000";
        public CombatlogSubevent SubEvent { get; set; } = CombatlogSubevent.UNDEFINED;
        public string SourceUID { get; set; } = "None-0000-00000000";
        public string SourceName { get; set; } = "Unnamed";
        public int SourceFlags { get; set; } = 0x0;
        public int SourceRaidFlags { get; set; } = 0x0;
        public string TargetUID { get; set; } = "None-0000-00000000";
        public string TargetName { get; set; } = "Unnamed";
        public int TargetFlags { get; set; } = 0x0;
        public int TargetRaidFlags { get; set; } = 0x0;

        public object[] Params { get; set; } = Array.Empty<object>();

    }
}
