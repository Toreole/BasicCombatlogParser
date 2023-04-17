namespace CombatlogParser.Data.Events;

class CastSuccessEvent : AdvancedParamEvent
{
    public int spellId;
    public string spellName;
    public SpellSchool spellSchool;

    public CastSuccessEvent( string entry, int dataIndex)
            : base(entry, ref dataIndex, EventType.CAST_SUCCESS, CombatlogEventPrefix.SPELL, CombatlogEventSuffix._CAST_SUCCESS)
    {
        GetSpellPrefixData(entry, ref dataIndex, out spellId, out spellName, out spellSchool);
        AdvancedParams = new(entry, ref dataIndex);
    }

    public override AdvancedParams AdvancedParams { get; protected set; }
}
