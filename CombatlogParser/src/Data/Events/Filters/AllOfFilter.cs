namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Composite filter that requires all supplied filters to match.
/// </summary>
public sealed class AllOfFilter : EventFilter
{
	private readonly EventFilter[] filters;
	public override bool Match(CombatlogEvent ev)
	{
		foreach (var f in filters)
			if (!f.Match(ev))
				return false;
		return true;
	}
	public AllOfFilter(params EventFilter[] filters)
		=> this.filters = filters;
}