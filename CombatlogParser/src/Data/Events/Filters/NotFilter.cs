namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Inverts the result of another filter.
/// </summary>
public sealed class NotFilter : EventFilter
{
	private readonly EventFilter filter;
	public override bool Match(CombatlogEvent ev) => !filter.Match(ev);
	public NotFilter(EventFilter filter)
	{
		this.filter = filter;
	}
}
