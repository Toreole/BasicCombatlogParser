namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Filters for events at or after the given timestamp.
/// </summary>
public sealed class AfterTimeFilter : EventFilter
{
    private readonly DateTime timestamp;
    public override bool Match(CombatlogEvent ev)
    {
        return ev.Timestamp >= timestamp;
    }
    public AfterTimeFilter(DateTime timestamp)
    {
        this.timestamp = timestamp;
    }
}
