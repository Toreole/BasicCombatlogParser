using CombatlogParser.Data.Events.EventData;

namespace CombatlogParser.Data.Events;

public interface ISpellEvent
{
	public SpellData SpellData { get; }
}
