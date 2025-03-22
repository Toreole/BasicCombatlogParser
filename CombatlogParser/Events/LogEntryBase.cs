namespace CombatlogParser.Events;

public abstract class LogEntryBase
{
	public DateTime Timestamp { get; set; } = DateTime.MinValue;
}
