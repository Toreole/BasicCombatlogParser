using CombatlogParser.Events.EventData;
using CombatlogParser.Data.WowEnums;
using CombatlogParser.Parsing;

namespace CombatlogParser.Events;

internal class DamageSupportEvent : AdvancedParamEvent, ISpellEvent
{
	//the leading bits of data.
	private readonly SpellData spellData;

	//what follows
	private readonly DamageEventParams damageParams;

	//whats important for Support
	private readonly string supporterGUID;

	public SpellData SpellData => spellData;
	public DamageEventParams DamageParams => damageParams;
	public string SupporterGUID => supporterGUID;

	public DamageSupportEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
		: base(entry, ref dataIndex, EventType.DAMAGE, prefix, CombatlogEventSuffix._DAMAGE)
	{
		spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
		AdvancedParams = new(entry, ref dataIndex);
		damageParams = new(entry, ref dataIndex);
		supporterGUID = string.Intern(ParsingUtil.NextSubstring(entry, ref dataIndex));
	}
}
