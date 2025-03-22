using CombatlogParser.Data.WowEnums;
using Windows.UI.WebUI;

namespace CombatlogParser.Events.Filters;

/// <summary>
/// Matches Targets who have any of the flags in the defined mask.
/// </summary>
public sealed class TargetAnyFlagFilter(UnitFlag mask) : EventFilter
{
	public override bool Match(CombatlogEvent ev)
		=> (ev.TargetFlags & mask) != UnitFlag.NONE;
}