using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events;

public class HealEvent : AdvancedParamEvent
{
    //spell/spell_periodic
    public int SpellId { get; }
    public string SpellName { get; }
    public SpellSchool SpellSchool { get; }

    //heal
    public int Amount { get; }
    public int BaseAmount { get; }
    public int Overheal { get; }
    public int Absorbed { get; }
    public bool Critical { get; }

    public HealEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) 
        : base(entry, ref dataIndex, EventType.HEALING, prefix, CombatlogEventSuffix._HEAL)
    {
        SpellId = int.Parse(NextSubstring(entry, ref dataIndex));
        SpellName = NextSubstring(entry, ref dataIndex);
        SpellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref dataIndex));
        AdvancedParams = new(entry, ref dataIndex);
        Amount = int.Parse(NextSubstring(entry, ref dataIndex));
        BaseAmount = int.Parse(NextSubstring(entry, ref dataIndex));
        Overheal = int.Parse(NextSubstring(entry, ref dataIndex));
        Absorbed = int.Parse(NextSubstring(entry, ref dataIndex));
        Critical = NextSubstring(entry, ref dataIndex) == "1";
    }
}
