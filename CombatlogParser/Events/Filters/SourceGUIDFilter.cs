namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters Sources given a set of GUIDs
/// </summary>
public sealed class SourceGUIDFilter(params string[] targets) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.SourceGUID);
	}
}
