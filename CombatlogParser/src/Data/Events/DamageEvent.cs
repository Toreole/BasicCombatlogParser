using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events
{
    public class DamageEvent : AdvancedParamEvent
    {
        //the leading bits of data.
        public SpellData spellData;

        //what follows
        public DamageEventParams damageParams;

        public DamageEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) 
            : base(entry, ref dataIndex, EventType.DAMAGE, prefix, CombatlogEventSuffix._DAMAGE)
        {
            spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
            AdvancedParams = new(entry, ref dataIndex);
            damageParams = new(entry, ref dataIndex);
        }
    }
}
