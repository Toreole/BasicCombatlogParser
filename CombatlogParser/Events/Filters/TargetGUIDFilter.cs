namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters Targets given a set of GUIDs
/// </summary>
public sealed class TargetGUIDFilter(params string[] targets) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.TargetGUID);
	}
}
