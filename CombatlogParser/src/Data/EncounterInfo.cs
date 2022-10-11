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
        /// The timestamp of the ENCOUNTER_START event
        /// </summary>
        public DateTime EncounterStartTime { get; set; }

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
    }
}
