using CombatlogParser.Data.WowEnums;

namespace CombatlogParser.Events.Filters;

/// <summary>
/// Filters for a specific set of flags on the source.
/// Events must match ALL flags.
/// </summary>
public sealed class SourceFlagFilter : EventFilter
{
	private readonly UnitFlag flags;
	public SourceFlagFilter(UnitFlag flags)
	{
		this.flags = flags;
	}
	public override bool Match(CombatlogEvent ev)
	{
		return (ev.SourceFlags & flags) == flags;
	}

	public static readonly SourceFlagFilter FriendlyPets = new(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID | UnitFlag.COMBATLOG_OBJECT_TYPE_PET);
	public static readonly SourceFlagFilter FriendlyGuardians = new(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID | UnitFlag.COMBATLOG_OBJECT_TYPE_GUARDIAN);
}
