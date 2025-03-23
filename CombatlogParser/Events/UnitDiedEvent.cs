using CombatlogParser.Data.WowEnums;

namespace CombatlogParser.Events;

[CombatlogEvent(CombatlogEventSuffix._DIED, CombatlogEventPrefix.UNIT)]
public class UnitDiedEvent: CombatlogEvent
{
	public override void SetDataFrom(string entry, int dataIndex, CombatlogEventPrefix prefix)
	{
		SetBasicCombatlogData(entry, ref dataIndex);
	}
}
