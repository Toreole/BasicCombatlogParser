using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events.Filters;

public class SpellFilter(SpellData spell) : EventFilter, IEventFilter<ISpellEvent>
{
	public override bool Match(CombatlogEvent combatlogEvent)
	{
		if (combatlogEvent is ISpellEvent ev)
		{
			return Match(ev);
		}
		return false;
	}

	public bool Match(ISpellEvent tEvent)
	{
		return tEvent.SpellData == spell;
	}
}
