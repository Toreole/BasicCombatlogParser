using CombatlogParser.Data.WowEnums;

namespace CombatlogParser.Events;

public class UnitDiedEvent(CombatlogEventPrefix prefix, string entry, int index) 
	: CombatlogEvent(entry, ref index, EventType.DEATH, prefix, CombatlogEventSuffix._DIED)
{
}
