using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events;
[CombatlogEvent(CombatlogEventSuffix._ABSORBED,
	allowedPrefixes: [
		CombatlogEventPrefix.SPELL,
		CombatlogEventPrefix.SPELL_PERIODIC,
		CombatlogEventPrefix.SWING
	]
)]
public class SpellAbsorbedEvent : CombatlogEvent, ISpellEvent
{
	//These will not be included when the cause for this event is a SWING_DAMAGE event with event.Absorbed > 0
	public SpellData AbsorbedSpellData { get; private set; } = null!;

	public string AbsorbCasterGUID { get; private set; } = null!;
	public string AbsorbCasterName { get; private set; } = null!;
	public UnitFlag AbsorbCasterFlags { get; private set; }
	public RaidFlag AbsorbCasterRFlags { get; private set; }

	public SpellData HealAbsorbSpellData { get; private set; } = null!;
	public long AbsorbedAmount { get; private set; }
	public long TotalAbsorb { get; private set; }
	public bool Critical { get; private set; }

	public SpellData SpellData => AbsorbedSpellData;

	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		int argumentCount = CountArguments(entry, dataIndex);
		SetBasicCombatlogData(entry, ref dataIndex);

		// Absorbed Spell Data isnt included in the entry if melee
		AbsorbedSpellData = (argumentCount < 13) ? 
			AbsorbedSpellData = SpellData.MeleeHit
			: SpellData.ParseOrGet(CombatlogEventPrefix.SPELL, entry, ref dataIndex);

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
