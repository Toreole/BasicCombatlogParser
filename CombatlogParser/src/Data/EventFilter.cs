namespace CombatlogParser.Data
{
    public interface IEventFilter
    {
        bool Match(CombatlogEvent combatlogEvent);
    }

    /// <summary>
    /// Filters events based on their subevents.
    /// </summary>
    public sealed class SubeventFilter : IEventFilter
    {
#pragma warning disable CS8618 
        //allowedPrefixes/Suffixes must contain non-null value is irrelevant, as theyre created inside the class
        //and im making sure that theyre either empty arrays, or have the proper values.
        private CombatlogEventPrefix[] allowedPrefixes;
        private bool anyPrefix;

        private CombatlogEventSuffix[] allowedSuffixes;
        private bool anySuffix;
        private SubeventFilter() { }
#pragma warning restore CS8618

        public bool Match(CombatlogEvent combatlogEvent)
        {
            bool matchPrefix = anyPrefix || allowedPrefixes.Contains(combatlogEvent.SubeventPrefix);
            bool matchSuffix = anySuffix || allowedSuffixes.Contains(combatlogEvent.SubeventSuffix);
            return matchPrefix && matchSuffix;
        }

        /// <summary>
        /// Allows any subevent with a _DAMAGE(_LANDED) suffix to pass.
        /// </summary>
        public static readonly SubeventFilter DamageEvents = new()
        {
            allowedPrefixes = Array.Empty<CombatlogEventPrefix>(),
            anyPrefix = true,

            allowedSuffixes = new[] { CombatlogEventSuffix._DAMAGE, CombatlogEventSuffix._DAMAGE_LANDED },
            anySuffix = false
        };

        /// <summary>
        /// Allows only SPELL_PERIODIC_DAMAGE subevents to pass.
        /// </summary>
        public static readonly SubeventFilter PeriodicDamageEvents = new()
        {
            allowedPrefixes = new[] { CombatlogEventPrefix.SPELL_PERIODIC },
            anyPrefix = false,

            allowedSuffixes = new[] { CombatlogEventSuffix._DAMAGE },
            anySuffix = false
        };

        /// <summary>
        /// Allows any subevent with a _HEAL(_ABSORBED) suffix to pass.
        /// </summary>
        public static readonly SubeventFilter HealingEvents = new()
        {
            allowedPrefixes = Array.Empty<CombatlogEventPrefix>(),
            anyPrefix = true,

            allowedSuffixes = new[] { CombatlogEventSuffix._HEAL, CombatlogEventSuffix._HEAL_ABSORBED },
            anySuffix = false
        };

        /// <summary>
        /// Allows all periodic healing and absorb to pass.
        /// </summary>
        public static readonly SubeventFilter PeriodicHealEvents = new()
        {
            allowedPrefixes = new[] { CombatlogEventPrefix.SPELL_PERIODIC },
            anyPrefix = false,

            allowedSuffixes = new[] { CombatlogEventSuffix._HEAL, CombatlogEventSuffix._HEAL_ABSORBED },
            anySuffix = false
        };
    }

    /// <summary>
    /// Filters Targets given a set of GUIDs
    /// </summary>
    public sealed class TargetGUIDFilter : IEventFilter
    {
        private string[] targets;
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
        private string[] targets;
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
        private UnitFlag flags;
        public TargetFlagFilter(UnitFlag flags)
        {
            this.flags = flags;
        }
        public bool Match(CombatlogEvent ev)
        {
            return ev.TargetFlags.HasFlagf(flags);
        }
    }

    /// <summary>
    /// Filters for a specific set of flags on the source.
    /// </summary>
    public sealed class SourceFlagFilter : IEventFilter
    {
        private UnitFlag flags;
        public SourceFlagFilter(UnitFlag flags)
        {
            this.flags = flags;
        }
        public bool Match(CombatlogEvent ev)
        {
            return ev.SourceFlags.HasFlagf(flags);
        }
    }
}