using CombatlogParser.Data.Events;

namespace CombatlogParser.Data
{
    public static class EventFilters
    {
        public static readonly AnyOfFilter AllySourceFilter = new(
            new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY),
            new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL)
        );
        public static readonly TargetFlagFilter EnemyTargetFilter = 
            new(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE);
        public static readonly AllOfFilter AllySourceEnemyTargetFilter = new AllOfFilter(
            AllySourceFilter,
            EnemyTargetFilter
        );
        public static readonly TargetFlagFilter AllyTargetFilter
            = new(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY);
        public static readonly AllOfFilter GroupMemberTargetFilter
            = new(AllyTargetFilter,
                new TargetAnyFlagFilter(
                    UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID |
                    UnitFlag.COMBATLOG_OBJECT_AFFILIATION_PARTY |
                    UnitFlag.COMBATLOG_OBJECT_AFFILIATION_MINE)
                );
    }

    public interface IEventFilter
    {
        bool Match(CombatlogEvent combatlogEvent);
    }

    /// <summary>
    /// Filters Targets given a set of GUIDs
    /// </summary>
    public sealed class TargetGUIDFilter : IEventFilter
    {
        private readonly string[] targets;
        public TargetGUIDFilter(string[] targets)
        {
            this.targets = targets;
        }
        public bool Match(CombatlogEvent ev)
        {
            return targets.Contains(ev.TargetGUID);
        }
    }

    /// <summary>
    /// Filters Targets given a set of Names
    /// </summary>
    public sealed class TargetNameFilter : IEventFilter
    {
        private readonly string[] targets;
        public TargetNameFilter(string[] targets)
        {
            this.targets = targets;
        }
        public bool Match(CombatlogEvent ev)
        {
            return targets.Contains(ev.TargetName);
        }
    }

    /// <summary>
    /// Filters for a specific set of flags on the target.
    /// </summary>
    public sealed class TargetFlagFilter : IEventFilter
    {
        private readonly UnitFlag flags;
        public TargetFlagFilter(UnitFlag flags)
        {
            this.flags = flags;
        }
        public bool Match(CombatlogEvent ev)
        {
            return ev.TargetFlags.HasFlagf(flags);
        }
    }

    public sealed class TargetAnyFlagFilter : IEventFilter
    {
        private readonly UnitFlag searchedFlags;
        public TargetAnyFlagFilter(UnitFlag searchFlags)
        {
            this.searchedFlags = searchFlags;
        }
        public bool Match(CombatlogEvent ev)
            => (ev.TargetFlags & searchedFlags) != UnitFlag.NONE;
    }

    /// <summary>
    /// Filters for a specific set of flags on the source.
    /// Events must match ALL flags.
    /// </summary>
    public sealed class SourceFlagFilter : IEventFilter
    {
        private readonly UnitFlag flags;
        public SourceFlagFilter(UnitFlag flags)
        {
            this.flags = flags;
        }
        public bool Match(CombatlogEvent ev)
        {
            return (ev.SourceFlags & flags) == flags;
        }

        public static readonly SourceFlagFilter FriendlyPets = new(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID | UnitFlag.COMBATLOG_OBJECT_TYPE_PET);
        public static readonly SourceFlagFilter FriendlyGuardians = new(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID | UnitFlag.COMBATLOG_OBJECT_TYPE_GUARDIAN);
    }

    /// <summary>
    /// Lets events pass that match any of the provided filters.
    /// </summary>
    public sealed class AnyOfFilter : IEventFilter
    {
        private readonly IEventFilter[] filters;
        public bool Match(CombatlogEvent ev)
        {
            foreach (IEventFilter f in filters)
                if (f.Match(ev))
                    return true;
            return false;
        }
        public AnyOfFilter(params IEventFilter[] filters)
        {
            if (filters.Length == 0)
                throw new ArgumentException("filters[] must not have a length of 0.");
            this.filters = filters;
        }
    }

    /// <summary>
    /// Inverts the result of another filter.
    /// </summary>
    public sealed class NotFilter : IEventFilter
    {
        private readonly IEventFilter filter;
        public bool Match(CombatlogEvent ev) => !filter.Match(ev);
        public NotFilter(IEventFilter filter)
        {
            this.filter = filter;
        }
    }

    /// <summary>
    /// Composite filter that requires all supplied filters to match.
    /// </summary>
    public sealed class AllOfFilter : IEventFilter
    {
        private readonly IEventFilter[] filters;
        public bool Match(CombatlogEvent ev)
        {
            foreach (var f in filters)
                if (!f.Match(ev))
                    return false;
            return true;
        }
        public AllOfFilter(params IEventFilter[] filters)
            => this.filters = filters;
    }
}