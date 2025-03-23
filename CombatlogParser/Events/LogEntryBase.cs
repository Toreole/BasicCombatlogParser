using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events;

/// <summary>
/// The basis for all Combatlog entries. Even those that are not directly related to combat.
/// </summary>
public abstract class LogEntryBase
{
	/// <summary>
	/// Timestamp of the event. 
	/// </summary>
	public DateTime Timestamp { get; private set; } = DateTime.MinValue;

	/// <summary>
	/// This sets the timestamp based on the entry.
	/// </summary>
	/// <param name="entry">one line of text from the combatlog file</param>
	protected void SetTimestampFrom(string entry)
	{
		Timestamp = StringTimestampToDateTime(entry[..entry.IndexOf(timestamp_end_seperator)]);
	}
}
