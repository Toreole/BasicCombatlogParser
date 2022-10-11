namespace CombatlogParser.Data
{
    public class LogEntryBase
    {
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public string SubeventName { get; set; } = "NONE";
    }
}
