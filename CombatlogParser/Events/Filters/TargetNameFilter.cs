namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters Targets given a set of Names
/// </summary>
public sealed class TargetNameFilter(string[] targets) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.TargetName);
	}
}
