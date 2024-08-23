namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Filters Targets given a set of Names
/// </summary>
public sealed class TargetNameFilter : EventFilter
{
	private readonly string[] targets;
	public TargetNameFilter(string[] targets)
	{
		this.targets = targets;
	}
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.TargetName);
	}
}
