namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters Sources given a set of Names
/// </summary>
public sealed class SourceNameFilter(string[] targets) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.SourceName);
	}
}
