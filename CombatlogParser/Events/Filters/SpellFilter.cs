using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events.Filters;

public class SpellFilter : EventFilter, IEventFilter<ISpellEvent>
{
	private readonly SpellData spell;
	public SpellFilter(SpellData spell)
	{
		this.spell = spell;
	}

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
