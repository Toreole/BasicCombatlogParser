namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Filters Targets given a set of GUIDs
/// </summary>
public sealed class TargetGUIDFilter : EventFilter
{
	private readonly string[] targets;
	public TargetGUIDFilter(params string[] targets)
	{
		this.targets = targets;
	}
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.TargetGUID);
	}
}
