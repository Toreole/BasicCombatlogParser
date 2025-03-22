namespace CombatlogParser.Events.Filters;

/// <summary>
/// Matches events that are before or at the given timestamp.
/// </summary>
public sealed class BeforeTimeFilter(DateTime timestamp) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return ev.Timestamp <= timestamp;
	}
}
