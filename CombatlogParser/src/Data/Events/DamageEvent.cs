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
        public int amount;
        public int overkill;
        public SpellSchool damageSchool;
        public int resisted;
        public int blocked;
        public int absorbed;
        public bool critical;
        public bool glancing;
        public bool crushing;
        public bool isOffHand;

        public DamageEvent(string subEvent, string entry, int dataIndex) : base(EventType.DAMAGE)
        {
            if(subEvent.StartsWithF("SWING"))
            {
                spellId = 1;
                spellName = "Melee";
                spellSchool = SpellSchool.Physical;
            }
            else if(subEvent.StartsWithF("RANGE"))
            {
                spellId = 75;
                spellName = "Auto Shot";
                spellSchool = SpellSchool.Physical;
            }
            else
            {
                spellId = int.Parse(NextSubstring(entry, ref dataIndex));
                spellName = NextSubstring(entry, ref dataIndex);
                spellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref dataIndex));
            }
            advancedParams = AdvancedParams.Get(entry, ref dataIndex);

            amount = int.Parse(NextSubstring(entry, ref dataIndex));
            overkill = int.Parse(NextSubstring(entry, ref dataIndex));
            damageSchool = spellSchool; MovePastNextDivisor(entry, ref dataIndex);
            resisted = int.Parse(NextSubstring(entry, ref dataIndex));
            blocked = int.Parse(NextSubstring(entry, ref dataIndex));
            absorbed = int.Parse(NextSubstring(entry, ref dataIndex));
            critical = NextSubstring(entry, ref dataIndex) == "1";
            glancing = NextSubstring(entry, ref dataIndex) == "1";
            crushing = NextSubstring(entry, ref dataIndex) == "1";
            isOffHand = NextSubstring(entry, ref dataIndex) == "1";
        }
    }
}
