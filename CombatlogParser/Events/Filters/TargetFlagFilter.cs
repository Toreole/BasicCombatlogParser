using CombatlogParser.Data.WowEnums;
using CombatlogParser.Parsing;

namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters for a specific set of flags on the target.
/// </summary>
public sealed class TargetFlagFilter(UnitFlag flags) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
	{
		return ev.TargetFlags.HasFlagf(flags);
	}
}
