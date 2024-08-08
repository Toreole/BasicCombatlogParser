namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Filters for a specific set of flags on the target.
/// </summary>
public sealed class TargetFlagFilter : EventFilter
{
    private readonly UnitFlag flags;
    public TargetFlagFilter(UnitFlag flags)
    {
        this.flags = flags;
    }
    public override bool Match(CombatlogEvent ev)
    {
        return ev.TargetFlags.HasFlagf(flags);
    }
}
