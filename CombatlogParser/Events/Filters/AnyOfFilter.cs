namespace CombatlogParser.Events.Filters;

/// <summary>
/// Lets events pass that match any of the provided filters.
/// </summary>
public sealed class AnyOfFilter : EventFilter
{
	private readonly EventFilter[] filters;
	public override bool Match(CombatlogEvent ev)
	{
		foreach (EventFilter f in filters)
			if (f.Match(ev))
				return true;
		return false;
	}
	public AnyOfFilter(params EventFilter[] filters)
	{
		if (filters.Length == 0)
			throw new ArgumentException("filters[] must not have a length of 0.");
		this.filters = filters;
	}
}
