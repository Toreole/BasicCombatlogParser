using CombatlogParser.Data.WowEnums;
using CombatlogParser.Events.EventData;

namespace CombatlogParser.Events;

public abstract class AdvancedParamEvent : CombatlogEvent
{
#pragma warning disable //AdvancedParams is going to be set by the inheriting class. I do not care that its not initialized here.
	protected AdvancedParamEvent(string entry, ref int dataIndex, EventType eventType, CombatlogEventPrefix prefix, CombatlogEventSuffix suffix) : base(entry, ref dataIndex, eventType, prefix, suffix) { }
#pragma warning restore
	public AdvancedParams AdvancedParams { get; protected set; }
}