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

            allowedSuffixes = new[] { CombatlogEventSuffix._DAMAGE }, //, CombatlogEventSuffix._DAMAGE_LANDED //exclude damage landed, because it duplicates damage in analysis
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

        /// <summary>
        /// SWING_MISSED events
        /// </summary>
        public static readonly SubeventFilter MissedSwingEvents = new()
        {
            allowedPrefixes = new[] { CombatlogEventPrefix.SWING },
            anyPrefix = false,

            allowedSuffixes = new[] { CombatlogEventSuffix._MISSED },
            anySuffix = false
        };

        /// <summary>
        /// SPELL_MISSED or SPELL_PERIODIC_MISSED
        /// </summary>
        public static readonly SubeventFilter MissedSpellEvents = new()
        {
            allowedPrefixes = new[] { CombatlogEventPrefix.SPELL, CombatlogEventPrefix.SPELL_PERIODIC },
            anyPrefix = false,

            allowedSuffixes = new[] { CombatlogEventSuffix._MISSED },
            anySuffix = false
        };

        /// <summary>
        /// SPELL_SUMMON
        /// </summary>
        public static readonly SubeventFilter SummonEvents = new()
        {
            allowedPrefixes = new[] { CombatlogEventPrefix.SPELL },
            anyPrefix = false,

            allowedSuffixes = new[] { CombatlogEventSuffix._SUMMON },
            anySuffix = false
        };

        /// <summary>
        /// SPELL_CAST_SUCCESS
        /// </summary>
        public static readonly SubeventFilter CastSuccessEvents = new()
        {
            allowedPrefixes = new[] { CombatlogEventPrefix.SPELL },
            anyPrefix = false,

            allowedSuffixes = new[] { CombatlogEventSuffix._CAST_SUCCESS },
            anySuffix = false
        };
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
    /// Filters for ABSORB/MISS missed type given the _MISSED subevent suffix
    /// </summary>
    public sealed class MissTypeFilter : IEventFilter
    {
        private MissType missType;
        public bool Match(CombatlogEvent ev)
        {
            return ev.SubeventSuffix == CombatlogEventSuffix._MISSED &&
                Enum.Parse<MissType>((string)ev.SuffixParam0) == missType;
        }

        public static readonly MissTypeFilter Absorbed = new()
        {
            missType = MissType.ABSORB
        };
        public static readonly MissTypeFilter Missed = new()
        {
            missType = MissType.MISS
        };
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
}