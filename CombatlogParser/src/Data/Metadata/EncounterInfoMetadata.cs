namespace CombatlogParser.Data.Metadata
{
    public sealed class EncounterInfoMetadata
    {
        /// <summary>
        /// unique ID of this encounter info. incrementing uint
        /// </summary>
        public uint encounterInfoUID;
        /// <summary>
        /// the log this encounter is a part of.
        /// </summary>
        public uint logID;
        /// <summary>
        /// the start index of the first event in the filestream.
        /// </summary>
        public ulong encounterStartIndex;
        /// <summary>
        /// the ID of the in-game encounter.
        /// </summary>
        public uint wowEncounterID;
        /// <summary>
        /// whether the encounter was completed successfully
        /// </summary>
        public bool success;
        /// <summary>
        /// The difficulty this encounter was recorded on.
        /// </summary>
        public int difficultyID;
    }
}
