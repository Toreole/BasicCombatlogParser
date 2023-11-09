using CombatlogParser.Data.Events.EventData;
using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events;

class SpellAbsorbedEvent : CombatlogEvent
{
    //These will not be included when the cause for this event is a SWING_DAMAGE event with event.Absorbed > 0
    public int AbsorbedSpellID { get; private set; }
    public string AbsorbedSpellName { get; private set; }
    public SpellSchool AbsorbedSpellSchool { get; private set; }

    public string AbsorbCasterGUID { get; private set; }
    public string AbsorbCasterName { get; private set; }
    public UnitFlag AbsorbCasterFlags { get; private set; }
    public RaidFlag AbsorbCasterRFlags { get; private set; }

    public int HealAbsorbSpellID { get; private set; }
    public string HealAbsorbSpellName { get; private set; }
    public SpellSchool HealAbsorbSpellSchool { get; private set; }
    public long AbsorbedAmount { get; private set; }
    public long TotalAbsorb { get; private set; }
    public bool Critical { get; private set; }

    //TODO: THIS ENTIRE FILE NEEDS UPDATING TO NEW STANDARDS IF POSSIBLE
    public SpellAbsorbedEvent(string entry, int dataIndex)
        : base(entry, ref dataIndex, EventType.SPELL_ABSORBED, CombatlogEventPrefix.SPELL, CombatlogEventSuffix._ABSORBED)
    {
        string[] eventData = SplitArgumentString(entry, dataIndex);
        int index = 0;
        if (eventData.Length < 13)
        {
            AbsorbedSpellID = 1;
            AbsorbedSpellName = "Melee";
            AbsorbedSpellSchool = SpellSchool.Physical;
        }
        else
        {
            AbsorbedSpellID = int.Parse(eventData[index++]);
            AbsorbedSpellName = string.Intern(eventData[index++]);
            AbsorbedSpellSchool = (SpellSchool)HexStringToUInt(eventData[index++]);
        }
        AbsorbCasterGUID = string.Intern(eventData[index++]);
        AbsorbCasterName = string.Intern(eventData[index++]);
		AbsorbCasterFlags = (UnitFlag)HexStringToUInt(eventData[index++]);
        AbsorbCasterRFlags = (RaidFlag)HexStringToUInt(eventData[index++]);

        HealAbsorbSpellID = int.Parse(eventData[index++]);
        HealAbsorbSpellName = string.Intern(eventData[index++]);
        HealAbsorbSpellSchool = (SpellSchool)HexStringToUInt(eventData[index++]);

        AbsorbedAmount = long.Parse(eventData[index++]);
        TotalAbsorb = long.Parse(eventData[index++]);
        Critical = eventData[index++] == "1";

    }
}
