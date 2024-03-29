﻿using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events
{
    public class SummonEvent : CombatlogEvent
    {
        //basically just a SPELL event, there is nothing special to it.
        public int spellId;
        public string spellName;
        public SpellSchool spellSchool;

        //should always be a SPELL_SUMMON
        public SummonEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
            : base(entry, ref dataIndex, EventType.SUMMON, prefix, CombatlogEventSuffix._SUMMON)
        {
            spellId = int.Parse(NextSubstring(entry, ref dataIndex));
            spellName = string.Intern(NextSubstring(entry, ref dataIndex));
            spellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref dataIndex));
        }
    }
}
