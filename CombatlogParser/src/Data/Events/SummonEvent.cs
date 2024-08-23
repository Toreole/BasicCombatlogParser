using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events
{
    public class SummonEvent : CombatlogEvent, ISpellEvent
    {
        //basically just a SPELL event, there is nothing special to it.
        private readonly SpellData spellData;

        public SpellData SpellData => spellData;

        //should always be a SPELL_SUMMON
        public SummonEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
            : base(entry, ref dataIndex, EventType.SUMMON, prefix, CombatlogEventSuffix._SUMMON)
        {
            spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
        }
    }
}
