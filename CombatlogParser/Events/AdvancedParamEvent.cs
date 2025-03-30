using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

public abstract class AdvancedParamEvent : CombatlogEvent
{
	/// <summary>
	/// This is null! until SetAdvancedParams is called!
	/// Make sure to have all advanced param events use that method in SetDataFrom(...)
	/// </summary>
	public AdvancedEventData AdvancedParams { get; protected set; } = null!;

	protected void SetAdvancedParams(string entry, ref int dataIndex)
	{
		AdvancedParams = new(entry, ref dataIndex);
	}
}