using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events
{
    public class DamageEvent : CombatlogEvent
    {
        //the leading bits of data.
        public int spellId;
        public string spellName;
        public SpellSchool spellSchool;

        //advanced params. these are optional.
        public AdvancedParams advancedParams;

        //what follows
        public uint amount;
        public uint overkill;
        public SpellSchool damageSchool;
        public uint resisted;
        public uint blocked;
        public uint absorbed;
        public bool critical;
        public bool glancing;
        public bool crushing;
        public bool isOffHand;

        public DamageEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) 
            : base(EventType.DAMAGE, prefix, CombatlogEventSuffix._DAMAGE)
        {
            if(prefix is CombatlogEventPrefix.SWING)
            {
                spellId = 1;
                spellName = "Melee";
                spellSchool = SpellSchool.Physical;
            }
            else if(prefix is CombatlogEventPrefix.RANGE)
            {
                spellId = 75;
                spellName = "Auto Shot";
                spellSchool = SpellSchool.Physical;
            }
            else //is CombatlogEventPrefix.SPELL
            {
                spellId = int.Parse(NextSubstring(entry, ref dataIndex));
                spellName = NextSubstring(entry, ref dataIndex);
                spellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref dataIndex));
            }
            advancedParams = AdvancedParams.Get(entry, ref dataIndex);

            amount = uint.Parse(NextSubstring(entry, ref dataIndex));
            overkill = uint.Parse(NextSubstring(entry, ref dataIndex));
            damageSchool = spellSchool; MovePastNextDivisor(entry, ref dataIndex);
            resisted = uint.Parse(NextSubstring(entry, ref dataIndex));
            blocked = uint.Parse(NextSubstring(entry, ref dataIndex));
            absorbed = uint.Parse(NextSubstring(entry, ref dataIndex));
            critical = NextSubstring(entry, ref dataIndex) == "1";
            glancing = NextSubstring(entry, ref dataIndex) == "1";
            crushing = NextSubstring(entry, ref dataIndex) == "1";
            isOffHand = NextSubstring(entry, ref dataIndex) == "1";
        }
    }
}
