using CombatlogParser.Data.Events;

namespace CombatlogParser.Data
{
    public class EncounterInfo
    {
        /// <summary>
        /// The name of the encounter, provided by ENCOUNTER_START
        /// </summary>
        public string EncounterName { get; }

        /// <summary>
        /// the ID of the encounter, provided by ENCOUNTER_START
        /// </summary>
        public EncounterId EncounterID { get; }

        /// <summary>
        /// The difficulty of the encounter. see https://wowpedia.fandom.com/wiki/DifficultyID 
        /// </summary>
        public DifficultyId DifficultyID { get; }

        /// <summary>
        /// The size of the group involved in the encounter.
        /// </summary>
        public int GroupSize { get; }

        /// <summary>
        /// ID if the instance the encounter is in. https://wowpedia.fandom.com/wiki/InstanceID
        /// </summary>
        public uint InstanceID { get; }

        /// <summary>
        /// The timestamp of the ENCOUNTER_START event
        /// </summary>
        public DateTime EncounterStartTime { get; }

        /// <summary>
        /// The timestamp of ENCOUNTER_END
        /// </summary>
        public DateTime EncounterEndTime { get; }

        /// <summary>
        /// The duration of the encounter in milliseconds, provided by ENCOUNTER_END
        /// </summary>
        public uint EncounterDuration { get; }

        /// <summary>
        /// Whether the encounter ended successfully, provided by ENCOUNTER_END
        /// </summary>
        public bool EncounterSuccess { get; }

        /// <summary>
        /// Size of the PlayerInfo[] is given by ENCOUNTER_START, followed by the data from COMBATANT_INFO
        /// </summary>
        public PlayerInfo[] Players { get; }

        /// <summary>
        /// List of NPCs found in the events.
        /// </summary>
        public List<NpcInfo> Npcs { get; }

        /// <summary>
        /// All combatlog events during the encounter.
        /// </summary>
        public CombatlogEvent[] CombatlogEvents { get; }

        /// <summary>
        /// Combatlog Events sorted by data type.
        /// </summary>
        public CombatlogEventDictionary CombatlogEventDictionary { get; }

        public Dictionary<string, string> SourceToOwnerGuidLookup { get; }

        /// <summary>
        /// The duration of the encounter in seconds. Used for DPS calculation.
        /// </summary>
        public float LengthInSeconds
        {
            get => EncounterDuration / 1000;
        }

        public EncounterInfo(
            CombatlogEvent[] allEvents,
            CombatlogEventDictionary eventDictionary,
            DateTime startTime,
            bool success,
            DifficultyId difficultyId,
            EncounterId encounterId,
            string encounterName,
            int groupSize,
            uint encounterDurationInMS,
            DateTime endTime,
            PlayerInfo[] players,
            List<NpcInfo> npcs) //could be an array aswell 
        {
            CombatlogEvents = allEvents;
            CombatlogEventDictionary = eventDictionary;
            EncounterStartTime = startTime;
            EncounterEndTime = endTime;
            Players = players;
            EncounterSuccess = success;
            DifficultyID = difficultyId;
            EncounterName = encounterName;
            EncounterID = encounterId;
            GroupSize = groupSize;
            EncounterDuration = encounterDurationInMS;
            Npcs = npcs;

            //initialize the lookup table
            SourceToOwnerGuidLookup = new();
            foreach (var summon in CombatlogEventDictionary.GetEvents<SummonEvent>())
            {
                //the summoned "pet" is the targetGUID of the event.
                if (SourceToOwnerGuidLookup.ContainsKey(summon.TargetGUID) == false)
                    SourceToOwnerGuidLookup.Add(summon.TargetGUID, summon.SourceGUID);
            }
            foreach (var e in CombatlogEventDictionary.GetEvents<AdvancedParamEvent>())
            {
                var sourceGUID = e.SourceGUID;
                //if the source unit is the advanced info unit
                if (sourceGUID != e.AdvancedParams.infoGUID)
                    continue;
                var owner = e.AdvancedParams.ownerGUID;
                //"000000000000" is the default GUID for "no owner".
                //regular GUIDs start with "Player", "Creature" or "Pet".
                if (SourceToOwnerGuidLookup.ContainsKey(sourceGUID) == false)
                    SourceToOwnerGuidLookup.Add(sourceGUID, owner[0] == '0' ? sourceGUID : owner);
            }
        }

        public CombatlogEvent? FirstEventForGUID(string guid)
            => CombatlogEvents.FirstOrDefault(x => x.SourceGUID == guid);

        public PlayerInfo? FindPlayerInfoByGUID(string sourceGUID)
        {
            foreach (var p in Players)
            {
                if (p.GUID == sourceGUID)
                    return p;
            }
            return null;
        }
    }
}
