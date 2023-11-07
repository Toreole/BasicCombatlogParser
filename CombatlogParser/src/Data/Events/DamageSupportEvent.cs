using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events;

internal class DamageSupportEvent : AdvancedParamEvent
{
    //the leading bits of data.
    public SpellData spellData;

    //what follows
    public DamageEventParams damageParams;

    //whats important for Support
    public string supporterGUID;

    public DamageSupportEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
        : base(entry, ref dataIndex, EventType.DAMAGE, prefix, CombatlogEventSuffix._DAMAGE)
    {
        spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
        AdvancedParams = new(entry, ref dataIndex);
        damageParams = new(entry, ref dataIndex);
        supporterGUID = string.Intern(ParsingUtil.NextSubstring(entry, ref dataIndex));
	}
}
