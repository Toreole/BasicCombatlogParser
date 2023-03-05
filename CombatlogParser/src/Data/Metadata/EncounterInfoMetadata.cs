using System.ComponentModel.DataAnnotations;

namespace CombatlogParser.Data.Metadata
{
    public sealed class EncounterInfoMetadata
    {
        /// <summary>
        /// unique ID of this encounter info. incrementing uint
        /// </summary>
        [Key]
        public uint Id { get; set; }
        /// <summary>
        /// the log this encounter is a part of.
        /// </summary>
        public uint CombatlogMetadataId { get; set; }
        //dependency
        public CombatlogMetadata? CombatlogMetadata { get; set; }
        /// <summary>
        /// the start index of the first event in the filestream.
        /// </summary>
        public long EncounterStartIndex { get; set; }
        /// <summary>
        /// the ID of the in-game encounter.
        /// </summary>
        public uint WowEncounterID { get; set; }
        /// <summary>
        /// whether the encounter was completed successfully
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// The difficulty this encounter was recorded on.
        /// </summary>
        public int DifficultyID { get; set; }
        /// <summary>
        /// The amount of lines from start to end of the encounter in the combatlog file.
        /// </summary>
        public int EncounterLengthInFile { get; set; }
    }
}
