namespace CombatlogParser.Data
{
    public abstract class LogEntryBase
    {
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
    }
}
