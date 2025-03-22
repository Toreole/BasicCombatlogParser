namespace CombatlogParser.Events.Filters;

/// <summary>
/// Composite filter that requires all supplied filters to match.
/// </summary>
public sealed class AllOfFilter(params EventFilter[] filters) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		foreach (var f in filters)
			if (!f.Match(ev))
				return false;
		return true;
	}
}