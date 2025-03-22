using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events;

class SpellAbsorbedEvent : CombatlogEvent, ISpellEvent
{
	//These will not be included when the cause for this event is a SWING_DAMAGE event with event.Absorbed > 0
	public SpellData AbsorbedSpellData { get; private set; }

	public string AbsorbCasterGUID { get; private set; }
	public string AbsorbCasterName { get; private set; }
	public UnitFlag AbsorbCasterFlags { get; private set; }
	public RaidFlag AbsorbCasterRFlags { get; private set; }

	public SpellData HealAbsorbSpellData { get; private set; }
	public long AbsorbedAmount { get; private set; }
	public long TotalAbsorb { get; private set; }
	public bool Critical { get; private set; }

	public SpellData SpellData => AbsorbedSpellData;

	//TODO: THIS ENTIRE FILE NEEDS UPDATING TO NEW STANDARDS IF POSSIBLE
	public SpellAbsorbedEvent(string entry, int dataIndex)
		: base(entry, ref dataIndex, EventType.SPELL_ABSORBED, CombatlogEventPrefix.SPELL, CombatlogEventSuffix._ABSORBED)
	{
		int argumentCount = CountArguments(entry, dataIndex);

		if (argumentCount < 13)
		{
			AbsorbedSpellData = SpellData.MeleeHit;

		}
		else
		{
			AbsorbedSpellData = SpellData.ParseOrGet(CombatlogEventPrefix.SPELL, entry, ref dataIndex);
		}
		AbsorbCasterGUID = string.Intern(NextSubstring(entry, ref dataIndex));
		AbsorbCasterName = string.Intern(NextSubstring(entry, ref dataIndex));
		AbsorbCasterFlags = (UnitFlag)HexStringToUInt(NextSubstring(entry, ref dataIndex));
		AbsorbCasterRFlags = (RaidFlag)HexStringToUInt(NextSubstring(entry, ref dataIndex));

		HealAbsorbSpellData = SpellData.ParseOrGet(CombatlogEventPrefix.SPELL, entry, ref dataIndex);

		AbsorbedAmount = long.Parse(NextSubstring(entry, ref dataIndex));
		TotalAbsorb = long.Parse(NextSubstring(entry, ref dataIndex));
		Critical = NextSubstring(entry, ref dataIndex) == "1";

	}
}
