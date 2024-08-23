using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatlogParser.Data.Metadata
{
	public sealed class EncounterInfoMetadata : EntityBase
	{
		/// <summary>
		/// unique ID of this encounter info. incrementing uint
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override uint Id { get; set; }

		/// <summary>
		/// the start index of the first event in the filestream.
		/// </summary>
		public long EncounterStartIndex { get; set; }

		/// <summary>
		/// the ID of the in-game encounter.
		/// </summary>
		public EncounterId WowEncounterId { get; set; }

		/// <summary>
		/// whether the encounter was completed successfully
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The difficulty this encounter was recorded on.
		/// </summary>
		public DifficultyId DifficultyId { get; set; }

		/// <summary>
		/// The amount of lines from start to end of the encounter in the combatlog file.
		/// </summary>
		public int EncounterLengthInFile { get; set; }

		public long EncounterDurationMS { get; set; }

		/// <summary>
		/// the log this encounter is a part of.
		/// </summary>
		public uint CombatlogMetadataId { get; set; }

		//the navigation property, CombatlogMetadataId is the foreign key for this.
		public CombatlogMetadata? CombatlogMetadata { get; set; }

		//this might not be necessary as they can be processed seperately.
		//public List<PerformanceMetadata> PerformanceMetadatas { get; set; } = new List<PerformanceMetadata>();
	}
}
