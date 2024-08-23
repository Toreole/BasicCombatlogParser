namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Filters Sources given a set of GUIDs
/// </summary>
public sealed class SourceGUIDFilter : EventFilter
{
	private readonly string[] targets;
	public SourceGUIDFilter(params string[] targets)
	{
		this.targets = targets;
	}
	public override bool Match(CombatlogEvent ev)
	{
		return targets.Contains(ev.SourceGUID);
	}
}
