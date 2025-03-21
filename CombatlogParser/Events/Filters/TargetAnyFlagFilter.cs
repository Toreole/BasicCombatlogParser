using CombatlogParser.Data.WowEnums;

namespace CombatlogParser.Events.Filters;

/// <summary>
/// Matches Targets who have any of the flags in the defined mask.
/// </summary>
public sealed class TargetAnyFlagFilter : EventFilter
{
	private readonly UnitFlag searchedFlags;
	public TargetAnyFlagFilter(UnitFlag mask)
	{
		searchedFlags = mask;
	}
	public override bool Match(CombatlogEvent ev)
		=> (ev.TargetFlags & searchedFlags) != UnitFlag.NONE;
}
