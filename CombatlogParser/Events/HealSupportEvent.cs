using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;
using CombatlogParser.Parsing;

namespace CombatlogParser.Events;

internal class HealSupportEvent : AdvancedParamEvent, ISpellEvent
{
	private readonly SpellData spellData;
	private readonly HealEventParams healParams;
	private readonly string supporterGUID;

	public SpellData SpellData => spellData;
	public HealEventParams HealParams => healParams;
	public string SupporterGUID => supporterGUID;

	public HealSupportEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
		: base(entry, ref dataIndex, EventType.HEALING, prefix, CombatlogEventSuffix._HEAL)
	{
		spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);

		AdvancedParams = new(entry, ref dataIndex);

		healParams = new(entry, ref dataIndex);
		supporterGUID = string.Intern(ParsingUtil.NextSubstring(entry, ref dataIndex));
	}
}