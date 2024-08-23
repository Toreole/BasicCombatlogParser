namespace CombatlogParser.Data.Events;

public class UnitDiedEvent : CombatlogEvent
{
	public UnitDiedEvent(CombatlogEventPrefix prefix, string entry, int index)
		: base(entry, ref index, EventType.DEATH, prefix, CombatlogEventSuffix._DIED)
	{
		//this event technically has a single integer as a payload, but it just doesn't matter at all.
	}
}
