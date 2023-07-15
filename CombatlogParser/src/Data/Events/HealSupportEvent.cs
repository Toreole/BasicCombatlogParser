using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events;

internal class HealSupportEvent : AdvancedParamEvent
{
    public readonly SpellData spell;
    public readonly HealEventParams healParams;
    public readonly string supporterGUID;

    public HealSupportEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
        : base(entry, ref dataIndex, EventType.HEALING, prefix, CombatlogEventSuffix._HEAL)
    {
        spell = SpellData.ParseOrGet(prefix, entry, ref dataIndex);

        AdvancedParams = new(entry, ref dataIndex);

        healParams = new(entry, ref dataIndex);
        supporterGUID = ParsingUtil.NextSubstring(entry, ref dataIndex);
    }
}