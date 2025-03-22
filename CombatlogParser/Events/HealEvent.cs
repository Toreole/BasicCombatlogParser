using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

public class HealEvent : AdvancedParamEvent, ISpellEvent
{
	//spell/spell_periodic
	private readonly SpellData spellData;

	//heal
	private readonly HealEventParams healParams;
	public int Amount => healParams.amount;
	public int BaseAmount => healParams.baseAmount;
	public int Overheal => healParams.overheal;
	public int Absorbed => healParams.absorbed;
	public bool Critical => healParams.critical;
	public HealEventParams HealParams => healParams;

	public SpellData SpellData => spellData;

	public HealEvent(CombatlogEventPrefix prefix, string entry, int dataIndex)
		: base(entry, ref dataIndex, EventType.HEALING, prefix, CombatlogEventSuffix._HEAL)
	{
		spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);

		AdvancedParams = new(entry, ref dataIndex);

		healParams = new(entry, ref dataIndex);
	}
}
