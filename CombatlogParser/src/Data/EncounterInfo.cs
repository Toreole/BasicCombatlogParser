namespace CombatlogParser.Data
{
    public class EncounterInfo
    {
        /// <summary>
        /// The name of the encounter, provided by ENCOUNTER_START
        /// </summary>
        public string EncounterName { get; set; } = "Unnamed";

        /// <summary>
        /// the ID of the encounter, provided by ENCOUNTER_START
        /// </summary>
        public uint EncounterID { get; set; }

        /// <summary>
        /// The difficulty of the encounter. see https://wowpedia.fandom.com/wiki/DifficultyID 
        /// </summary>
        public int DifficultyID { get; set; }

        /// <summary>
        /// The size of the group involved in the encounter.
        /// </summary>
        public int GroupSize { get; set; }

        /// <summary>
        /// ID if the instance the encounter is in. https://wowpedia.fandom.com/wiki/InstanceID
        /// </summary>
        public uint InstanceID { get; set; }

        /// <summary>
        /// The timestamp of the ENCOUNTER_START event
        /// </summary>
        public DateTime EncounterStartTime { get; set; }

        /// <summary>
        /// The timestamp of ENCOUNTER_END
        /// </summary>
        public DateTime EncounterEndTime { get; set; }

        /// <summary>
        /// The duration of the encounter in milliseconds, provided by ENCOUNTER_END
        /// </summary>
        public uint EncounterDuration { get; set; }

        /// <summary>
        /// Whether the encounter ended successfully, provided by ENCOUNTER_END
        /// </summary>
        public bool EncounterSuccess { get; set; } = false;

        /// <summary>
        /// Size of the PlayerInfo[] is given by ENCOUNTER_START, followed by the data from COMBATANT_INFO
        /// </summary>
        public PlayerInfo[] Players { get; set; } = Array.Empty<PlayerInfo>();

        /// <summary>
        /// All combatlog events during the encounter.
        /// </summary>
        public CombatlogEvent[] CombatlogEvents { get; set; } = Array.Empty<CombatlogEvent>();

        /// <summary>
        /// The duration of the encounter in seconds. Used for DPS calculation.
        /// </summary>
        public float LengthInSeconds
        {
            get => EncounterDuration / 1000;
        }

        /// <summary>
        /// Gets all events that match the provided filters.
        /// </summary>
        /// <exception cref="ArgumentException">filters must not be empty</exception>
        public CombatlogEvent[] AllEventsThatMatch(params IEventFilter[] filters)
        {
            if (filters.Length == 0)
                throw new ArgumentException("argument 'filters' must not be empty.");
            //make the match too big at first 
            CombatlogEvent[] matchingEvents = new CombatlogEvent[CombatlogEvents.Length];
            int matchingCount = 0;
            foreach(CombatlogEvent ev in CombatlogEvents)
            {
                foreach (var filter in filters)
                {
                    if (filter.Match(ev) == false)
                        goto nextOuter; //skip to end of outer foreach. continue doesnt work here
                }
                matchingEvents[matchingCount] = ev;
                matchingCount++;
            nextOuter:
                continue;
            }
            return matchingEvents[0..matchingCount];
        }
    }
}
