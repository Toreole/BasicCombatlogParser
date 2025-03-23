using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;
using CombatlogParser.Parsing;

namespace CombatlogParser.Events;

[CombatlogEvent(CombatlogEventSuffix._HEAL_SUPPORT,
	allowedPrefixes: [
		CombatlogEventPrefix.SPELL,
		CombatlogEventPrefix.SPELL_PERIODIC
	]
)]
internal class HealSupportEvent : AdvancedParamEvent, ISpellEvent
{
	public SpellData SpellData { get; private set; } = null!;
	public HealEventParams HealParams { get; private set; } = null!;
	public string SupporterGUID { get; private set; } = null!;

	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		SetBasicCombatlogData(entry, ref dataIndex);
		SpellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
		SetAdvancedParams(entry, ref dataIndex);
		HealParams = new(entry, ref dataIndex);
		SupporterGUID = string.Intern(ParsingUtil.NextSubstring(entry, ref dataIndex));
	}
}