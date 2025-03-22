namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters for events at or after the given timestamp.
/// </summary>
public sealed class AfterTimeFilter(DateTime timestamp) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return ev.Timestamp >= timestamp;
	}
}
