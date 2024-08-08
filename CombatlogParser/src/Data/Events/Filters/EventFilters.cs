namespace CombatlogParser.Data.Events.Filters;

/// <summary>
/// Helper class for ease of use. 
/// Place to store commonly used filters and factory methods so you dont have to memorize individual filter names.
/// </summary>
public static class EventFilters
{
    /// <summary>
    /// Filters for Sources with the REACTION_FRIENDLY or REACTION_NEUTRAL flags
    /// </summary>
    public static readonly AnyOfFilter AllySourceFilter = new(
        new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY),
        new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL));

    /// <summary>
    /// Filters for Targets with the REACTION_HOSTILE flag
    /// </summary>
    public static readonly TargetFlagFilter EnemyTargetFilter =
        new(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE);

    /// <summary>
    /// Combination of AllySource and EnemyTarget
    /// </summary>
    public static readonly AllOfFilter AllySourceEnemyTargetFilter = new(
        AllySourceFilter,
        EnemyTargetFilter);

    /// <summary>
    /// Filters for Targets with the REACTION_FRIENDLY flag
    /// </summary>
    public static readonly TargetFlagFilter AllyTargetFilter
        = new(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY);

    /// <summary>
    /// Filters for Targets who are in a group / raid group with the player or the player themselves.
    /// </summary>
    public static readonly AllOfFilter GroupMemberTargetFilter
        = new(AllyTargetFilter,
            new TargetAnyFlagFilter(
                UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID |
                UnitFlag.COMBATLOG_OBJECT_AFFILIATION_PARTY |
                UnitFlag.COMBATLOG_OBJECT_AFFILIATION_MINE));

    /// <summary>
    /// Filters for Sources with the REACTION_HOSTILE and OBJECT_TYPE_NPC flags.
    /// </summary>
    public static readonly AllOfFilter EnemySourceFilter = new(
        new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE),
        new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_TYPE_NPC));

    public static EventFilter Before(DateTime dateTime) => new BeforeTimeFilter(dateTime);
    public static EventFilter After(DateTime dateTime) => new AfterTimeFilter(dateTime);
}
