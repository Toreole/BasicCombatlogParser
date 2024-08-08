namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Filters Sources given a set of Names
/// </summary>
public sealed class SourceNameFilter : EventFilter
{
    private readonly string[] targets;
    public SourceNameFilter(string[] targets)
    {
        this.targets = targets;
    }
    public override bool Match(CombatlogEvent ev)
    {
        return targets.Contains(ev.SourceName);
    }
}
