using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events;

class CastSuccessEvent : AdvancedParamEvent, ISpellEvent
{
    public SpellData SpellData { get; private set; }

    public CastSuccessEvent(string entry, int dataIndex)
            : base(entry, ref dataIndex, EventType.CAST_SUCCESS, CombatlogEventPrefix.SPELL, CombatlogEventSuffix._CAST_SUCCESS)
    {
        SpellData = SpellData.ParseOrGet(CombatlogEventPrefix.SPELL, entry, ref dataIndex);
        AdvancedParams = new(entry, ref dataIndex);
    }

}
