using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events;

public class HealEvent : AdvancedParamEvent
{
    //spell/spell_periodic
    public SpellData spell;

    //heal
    public readonly HealEventParams healParams;
    public int Amount => healParams.amount;
    public int BaseAmount => healParams.baseAmount;
    public int Overheal => healParams.overheal;
    public int Absorbed => healParams.absorbed;
    public bool Critical => healParams.critical;

    public HealEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) 
        : base(entry, ref dataIndex, EventType.HEALING, prefix, CombatlogEventSuffix._HEAL)
    {
        spell = SpellData.ParseOrGet(prefix, entry, ref dataIndex);

        AdvancedParams = new(entry, ref dataIndex);

        healParams = new(entry, ref dataIndex);
    }
}
