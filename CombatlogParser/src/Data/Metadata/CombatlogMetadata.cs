using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatlogParser.Data.Metadata
{
    [Table("CombatlogMetadatas")]
    [PrimaryKey("Id")]
    public class CombatlogMetadata : EntityBase
    {
        /// <summary>
        /// unique ID (incrementing int) for easier referencing from other data. <br />
        /// This is set by the DB. changes will not affect this.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override uint Id { get; set; }

        /// <summary>
        /// The name of the file of the combatlog. Must be UNIQUE and can be used as the PRIMARY KEY
        /// </summary>
        [Key]
        public string FileName { get; set; } = "";

        /// <summary>
        /// Whether the log is advanced
        /// </summary>
        public bool IsAdvanced { get; set; }

        /// <summary>
        /// the unix milliseconds timestamp for the start of the log.
        /// </summary>
        public long MsTimeStamp { get; set; }

        /// <summary>
        /// The version of the build thats responsible for this log.
        /// </summary>
        public string BuildVersion { get; set; } = "";

        /// <summary>
        /// the project ID
        /// </summary>
        public WowProjectID ProjectID { get; set; }

        //Inverse navigation property. is populated by EncounterInfoMetadata entities that refer to this one.
        public virtual List<EncounterInfoMetadata> Encounters { get; set; } = new();

		public override string ToString()
		{
            return $"({Id}){FileName}";
		}
	}
}
