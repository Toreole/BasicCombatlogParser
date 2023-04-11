using CombatlogParser.Data.Events;

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
        public EncounterId EncounterID { get; set; }

        /// <summary>
        /// The difficulty of the encounter. see https://wowpedia.fandom.com/wiki/DifficultyID 
        /// </summary>
        public DifficultyId DifficultyID { get; set; }

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
        public PlayerInfo[] Players { get; init; } = Array.Empty<PlayerInfo>();

        /// <summary>
        /// List of NPCs found in the events.
        /// </summary>
        public List<NpcInfo> Npcs { get; init; } = new List<NpcInfo>();

        /// <summary>
        /// All combatlog events during the encounter.
        /// </summary>
        public CombatlogEvent[] CombatlogEvents { get; init; } = Array.Empty<CombatlogEvent>();
        //TODO: - actually create this and store it, then later use it for most things instead of CombatlogEvents.
        public CombatlogEventDictionary CombatlogEventDictionary { get; init; } = new();

        /// <summary>
        /// The duration of the encounter in seconds. Used for DPS calculation.
        /// </summary>
        public float LengthInSeconds
        {
            get => EncounterDuration / 1000;
        }

        public CombatlogEvent? FirstEventForGUID(string guid)
        {
            foreach (var e in CombatlogEvents)
                if (e.SourceGUID == guid)
                    return e;
            return null;
        }

        public PlayerInfo? FindPlayerInfoByGUID(string sourceGUID)
        {
            foreach(var p in Players)
            {
                if (p.GUID == sourceGUID)
                    return p;
            }
            return null;
        }
    }
}
