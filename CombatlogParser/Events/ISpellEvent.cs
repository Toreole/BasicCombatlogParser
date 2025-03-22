using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

public interface ISpellEvent
{
	public SpellData SpellData { get; }
}
