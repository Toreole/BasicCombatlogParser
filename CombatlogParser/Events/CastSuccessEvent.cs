using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

[CombatlogEvent(CombatlogEventSuffix._CAST_SUCCESS, allowedPrefixes: [CombatlogEventPrefix.SPELL])]
class CastSuccessEvent : AdvancedParamEvent, ISpellEvent
{
	public SpellData SpellData { get; private set; } = null!;

	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		SetBasicCombatlogData(entry, ref dataIndex);
		// SpellData is sourced from the prefix.
		SpellData = SpellData.ParseOrGet(CombatlogEventPrefix.SPELL, entry, ref dataIndex);
		SetAdvancedParams(entry, ref dataIndex);
	}

}
