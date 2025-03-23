using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

[CombatlogEvent(CombatlogEventSuffix._HEAL,
	allowedPrefixes: [
		CombatlogEventPrefix.SPELL,
		CombatlogEventPrefix.SPELL_PERIODIC
	]
)]
public class HealEvent : AdvancedParamEvent, ISpellEvent
{
	//spell/spell_periodic
	public SpellData SpellData { get; private set; } = null!;

	//heal
	public HealEventParams HealParams { get; private set; } = null!;

	// shortcuts.
	public int Amount => HealParams.amount;
	public int BaseAmount => HealParams.baseAmount;
	public int Overheal => HealParams.overheal;
	public int Absorbed => HealParams.absorbed;
	public bool Critical => HealParams.critical;


	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		SetBasicCombatlogData(entry, ref dataIndex);
	 	SpellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
		SetAdvancedParams(entry, ref dataIndex);
		HealParams = new(entry, ref dataIndex);
	}
}
