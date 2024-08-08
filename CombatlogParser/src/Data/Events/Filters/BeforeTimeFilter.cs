namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Matches events that are before or at the given timestamp.
/// </summary>
public sealed class BeforeTimeFilter : EventFilter
{
    private readonly DateTime timestamp;
    public override bool Match(CombatlogEvent ev)
    {
        return ev.Timestamp <= timestamp;
    }
    public BeforeTimeFilter(DateTime timestamp)
    {
        this.timestamp = timestamp;
    }
}
