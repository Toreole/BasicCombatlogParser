using CombatlogParser.Data.Events;
using System.Runtime.CompilerServices;
using Windows.Foundation.Metadata;

namespace CombatlogParser.Data
{
    /// <summary>
    /// Helper class for ease of use. 
    /// Place to store commonly used filters and factory methods so you dont have to memorize individual filter names.
    /// </summary>
    public static class EventFilters
    {
        public static readonly AnyOfFilter AllySourceFilter = new(
            new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY),
            new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL)
        );
        public static readonly TargetFlagFilter EnemyTargetFilter =
            new(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE);
        public static readonly AllOfFilter AllySourceEnemyTargetFilter = new(
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
        public static EventFilter AllOf(params EventFilter[] filters) => new AllOfFilter(filters);
        public static EventFilter AnyOf(params EventFilter[] filters) => new AnyOfFilter(filters);

        public static EventFilter Before(DateTime dateTime) => new BeforeTimeFilter(dateTime);
        public static EventFilter After(DateTime dateTime) => new BeforeTimeFilter(dateTime);
    }

    public abstract class EventFilter
    {
        public abstract bool Match(CombatlogEvent combatlogEvent);

        public static explicit operator EventFilter(EventFilter[] filters) {
            return new AllOfFilter(filters);
        }
    }

    public sealed class BeforeTimeFilter : EventFilter
    {
        private readonly DateTime timestamp;
        public override bool Match(CombatlogEvent ev)
        {
            return ev.Timestamp <= timestamp;
        }
        public BeforeTimeFilter(DateTime timestamp)
        {
            this.timestamp = timestamp;
        }
    }
    public sealed class AfterTimeFilter : EventFilter
    {
        private readonly DateTime timestamp;
        public override bool Match(CombatlogEvent ev)
        {
            return ev.Timestamp >= timestamp;
        }
        public AfterTimeFilter(DateTime timestamp)
        {
            this.timestamp = timestamp;
        }
    }

    /// <summary>
    /// Filters Targets given a set of GUIDs
    /// </summary>
    public sealed class TargetGUIDFilter : EventFilter
    {
        private readonly string[] targets;
        public TargetGUIDFilter(params string[] targets)
        {
            this.targets = targets;
        }
        public override bool Match(CombatlogEvent ev)
        {
            return targets.Contains(ev.TargetGUID);
        }
    }

    /// <summary>
    /// Filters Targets given a set of Names
    /// </summary>
    public sealed class TargetNameFilter : EventFilter
    {
        private readonly string[] targets;
        public TargetNameFilter(string[] targets)
        {
            this.targets = targets;
        }
        public override bool Match(CombatlogEvent ev)
        {
            return targets.Contains(ev.TargetName);
        }
    }

    /// <summary>
    /// Filters for a specific set of flags on the target.
    /// </summary>
    public sealed class TargetFlagFilter : EventFilter
    {
        private readonly UnitFlag flags;
        public TargetFlagFilter(UnitFlag flags)
        {
            this.flags = flags;
        }
        public override bool Match(CombatlogEvent ev)
        {
            return ev.TargetFlags.HasFlagf(flags);
        }
    }

    public sealed class TargetAnyFlagFilter : EventFilter
    {
        private readonly UnitFlag searchedFlags;
        public TargetAnyFlagFilter(UnitFlag searchFlags)
        {
            this.searchedFlags = searchFlags;
        }
        public override bool Match(CombatlogEvent ev)
            => (ev.TargetFlags & searchedFlags) != UnitFlag.NONE;
    }

	/// <summary>
	/// Filters Sources given a set of GUIDs
	/// </summary>
	public sealed class SourceGUIDFilter : EventFilter
	{
		private readonly string[] targets;
		public SourceGUIDFilter(params string[] targets)
		{
			this.targets = targets;
		}
		public override bool Match(CombatlogEvent ev)
		{
			return targets.Contains(ev.SourceGUID);
		}
	}

	/// <summary>
	/// Filters Sources given a set of Names
	/// </summary>
	public sealed class SourceNameFilter : EventFilter
	{
		private readonly string[] targets;
		public SourceNameFilter(string[] targets)
		{
			this.targets = targets;
		}
		public override bool Match(CombatlogEvent ev)
		{
			return targets.Contains(ev.SourceName);
		}
	}

	/// <summary>
	/// Filters for a specific set of flags on the source.
	/// Events must match ALL flags.
	/// </summary>
	public sealed class SourceFlagFilter : EventFilter
    {
        private readonly UnitFlag flags;
        public SourceFlagFilter(UnitFlag flags)
        {
            this.flags = flags;
        }
        public override bool Match(CombatlogEvent ev)
        {
            return (ev.SourceFlags & flags) == flags;
        }

        public static readonly SourceFlagFilter FriendlyPets = new(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID | UnitFlag.COMBATLOG_OBJECT_TYPE_PET);
        public static readonly SourceFlagFilter FriendlyGuardians = new(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID | UnitFlag.COMBATLOG_OBJECT_TYPE_GUARDIAN);
    }

    /// <summary>
    /// Lets events pass that match any of the provided filters.
    /// </summary>
    public sealed class AnyOfFilter : EventFilter
    {
        private readonly EventFilter[] filters;
        public override bool Match(CombatlogEvent ev)
        {
            foreach (EventFilter f in filters)
                if (f.Match(ev))
                    return true;
            return false;
        }
        public AnyOfFilter(params EventFilter[] filters)
        {
            if (filters.Length == 0)
                throw new ArgumentException("filters[] must not have a length of 0.");
            this.filters = filters;
        }
    }

    /// <summary>
    /// Inverts the result of another filter.
    /// </summary>
    public sealed class NotFilter : EventFilter
    {
        private readonly EventFilter filter;
        public override bool Match(CombatlogEvent ev) => !filter.Match(ev);
        public NotFilter(EventFilter filter)
        {
            this.filter = filter;
        }
    }

    /// <summary>
    /// Composite filter that requires all supplied filters to match.
    /// </summary>
    public sealed class AllOfFilter : EventFilter
    {
        private readonly EventFilter[] filters;
        public override bool Match(CombatlogEvent ev)
        {
            foreach (var f in filters)
                if (!f.Match(ev))
                    return false;
            return true;
        }
        public AllOfFilter(params EventFilter[] filters)
            => this.filters = filters;
    }
}