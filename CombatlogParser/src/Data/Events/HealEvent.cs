using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events
{
    public class HealEvent : CombatlogEvent
    {
        //spell/spell_periodic
        public int SpellId { get; init; }
        public string SpellName { get; init; }
        public SpellSchool SpellSchool { get; init; }

        public AdvancedParams AdvancedParams { get; init; }

        //heal
        public int Amount { get; init; }
        public int BaseAmount { get; init; }
        public int Overheal { get; init; }
        public int Absorbed { get; init; }
        public bool Critical { get; init; }

        public HealEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) : base(EventType.HEALING, prefix, CombatlogEventSuffix._HEAL)
        {
            base.ReadData(entry, ref dataIndex);
            SpellId = int.Parse(NextSubstring(entry, ref dataIndex));
            SpellName = NextSubstring(entry, ref dataIndex);
            SpellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref dataIndex));
            AdvancedParams = AdvancedParams.Get(entry, ref dataIndex);
            Amount = int.Parse(NextSubstring(entry, ref dataIndex));
            BaseAmount = int.Parse(NextSubstring(entry, ref dataIndex));
            Overheal = int.Parse(NextSubstring(entry, ref dataIndex));
            Absorbed = int.Parse(NextSubstring(entry, ref dataIndex));
            Critical = NextSubstring(entry, ref dataIndex) == "1";
        }
    }
}
