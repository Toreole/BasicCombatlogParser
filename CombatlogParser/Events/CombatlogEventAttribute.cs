using CombatlogParser.Data.WowEnums;

namespace CombatlogParser.Events;

/// <summary>
/// Denotes that an Event class relates to a given set of prefixes and suffix
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CombatlogEventAttribute(CombatlogEventSuffix targetSuffix, params CombatlogEventPrefix[] allowedPrefixes) : Attribute
{
	public CombatlogEventPrefix[] AllowedPrefixes { get; } = allowedPrefixes;
	public CombatlogEventSuffix TargetSuffix { get; } = targetSuffix;
}
