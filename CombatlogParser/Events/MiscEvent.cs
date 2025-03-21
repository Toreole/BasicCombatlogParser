using CombatlogParser.Data.WowEnums;

namespace CombatlogParser.Events
{
	public class MiscEvent : LogEntryBase
	{
		public CombatlogMiscEvents Event { get; init; }
	}
}
