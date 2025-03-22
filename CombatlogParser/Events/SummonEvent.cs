using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events
{
	public class SummonEvent(CombatlogEventPrefix prefix, string entry, int dataIndex) 
		: CombatlogEvent(entry, ref dataIndex, EventType.SUMMON, prefix, CombatlogEventSuffix._SUMMON), ISpellEvent
	{
		//basically just a SPELL event, there is nothing special to it.
		private readonly SpellData spellData = SpellData.ParseOrGet(prefix, entry, ref dataIndex);

		public SpellData SpellData => spellData;
	}
}
