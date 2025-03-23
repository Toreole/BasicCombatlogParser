using CombatlogParser.Data.WowEnums;
using CombatlogParser.Parsing;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events;

/// <summary>
/// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
/// </summary>
public abstract class CombatlogEvent : LogEntryBase
{
	public CombatlogEventPrefix SubeventPrefix { get; private set; } = CombatlogEventPrefix.UNDEFINED;

	//these 8 parameters are guaranteed to be included in combatlog events.
	public string SourceGUID { get; private set; } = string.Empty;
	public string SourceName { get; private set; } = string.Empty;
	public UnitFlag SourceFlags { get; private set; }
	public RaidFlag SourceRaidFlags { get; private set; }
	public string TargetGUID { get; private set; } = string.Empty;
	public string TargetName { get; private set; } = string.Empty;
	public UnitFlag TargetFlags { get; private set; }
	public RaidFlag TargetRaidFlags { get; private set; }

	// this may already be obsolete.
	public EventType EventType { get; private set; } = EventType.UNDEFINED;

	/// <summary>
	/// Whether the source is a pet type object in the game.
	/// This does not tell the full story in regards to the "actual source" of damage or healing.
	/// For that, the AdvancedParams are to be used, specifically the OwnerGUID.
	/// </summary>
	public bool SourceIsPet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

	/// <summary>
	/// Initializes all fields in the CombatlogEvent.
	/// Suffix is omitted as it is largely implicit.
	/// </summary>
	/// <param name="entry">One full line of the combatlog file.</param>
	/// <param name="dataIndex">Index, where the data is in the entry.</param>
	/// <param name="prefix">The actual prefix of the event.</param>
	public abstract void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix);

	/// <summary>
	/// Initializes the basic CombatlogEvent data. Timestamp, Source info, Target info.
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="dataIndex"></param>
	protected void SetBasicCombatlogData(string entry, ref int dataIndex)
	{
		SetTimestampFrom(entry);
		// Source
		SourceGUID = string.Intern(NextSubstring(entry, ref dataIndex));
		SourceName = string.Intern(NextSubstring(entry, ref dataIndex));
		SourceFlags = NextFlags(entry, ref dataIndex);
		SourceRaidFlags = NextRaidFlags(entry, ref dataIndex);
		// Target
		TargetGUID = string.Intern(NextSubstring(entry, ref dataIndex));
		TargetName = string.Intern(NextSubstring(entry, ref dataIndex));
		TargetFlags = NextFlags(entry, ref dataIndex);
		TargetRaidFlags = NextRaidFlags(entry, ref dataIndex);
	}
}