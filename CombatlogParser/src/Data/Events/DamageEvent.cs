using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events
{
    public class DamageEvent : CombatlogEvent
    {
        //the leading bits of data.
        public int spellId;
        public string spellName { get; private set; } //these get/set properties are a temporary measure to display stuff on-screen.
        public SpellSchool spellSchool { get; private set; }

        //advanced params. these are optional.
        public AdvancedParams advancedParams;

        //what follows
        public long Amount { get; private set; }
        public long baseAmount { get; private set; }
        public long overkill { get; private set; }
        public SpellSchool damageSchool { get; private set; }
        public long resisted;
        public long blocked;
        public long Absorbed;
        public bool critical;
        public bool glancing;
        public bool crushing;
        public bool isOffHand;

        public DamageEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) 
            : base(EventType.DAMAGE, prefix, CombatlogEventSuffix._DAMAGE)
        {
            base.ReadData(entry, ref dataIndex);
            if(prefix is CombatlogEventPrefix.SWING)
            {
                spellId = 1;
                spellName = "Melee";
                spellSchool = SpellSchool.Physical;
            }
            //else if(prefix is CombatlogEventPrefix.RANGE)
            //{
            //    spellId = 75;
            //    spellName = "Auto Shot";
            //    spellSchool = SpellSchool.Physical;
            //}
            else //is CombatlogEventPrefix.SPELL
            {
                spellId = int.Parse(NextSubstring(entry, ref dataIndex));
                spellName = NextSubstring(entry, ref dataIndex);
                spellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref dataIndex));
            }
            advancedParams = AdvancedParams.Get(entry, ref dataIndex);

            Amount = long.Parse(NextSubstring(entry, ref dataIndex));
            baseAmount = long.Parse(NextSubstring(entry, ref dataIndex));
            overkill = long.Parse(NextSubstring(entry, ref dataIndex));
            damageSchool = spellSchool; MovePastNextDivisor(entry, ref dataIndex);
            resisted = long.Parse(NextSubstring(entry, ref dataIndex));
            blocked = long.Parse(NextSubstring(entry, ref dataIndex));
            Absorbed = long.Parse(NextSubstring(entry, ref dataIndex));
            critical = NextSubstring(entry, ref dataIndex) == "1";
            glancing = NextSubstring(entry, ref dataIndex) == "1";
            crushing = NextSubstring(entry, ref dataIndex) == "1";
            isOffHand = NextSubstring(entry, ref dataIndex) == "1";
        }
    }
}
