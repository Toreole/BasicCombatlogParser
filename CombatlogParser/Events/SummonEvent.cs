using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

[CombatlogEvent(CombatlogEventSuffix._SUMMON, CombatlogEventPrefix.SPELL)]
public class SummonEvent: CombatlogEvent, ISpellEvent
{
	//basically just a SPELL event, there is nothing special to it.
	public SpellData SpellData { get; private set; } = null!;

	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		SetBasicCombatlogData(entry, ref dataIndex);
		SpellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);
	}

}
