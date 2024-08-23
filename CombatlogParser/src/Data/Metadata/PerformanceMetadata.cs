using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatlogParser.Data.Metadata;

public class PerformanceMetadata : EntityBase
{
	/// <summary>
	/// unique ID of the performance. autoincrement
	/// </summary>
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public override uint Id { get; set; }

	/// <summary>
	/// damage per second
	/// </summary>
	public double Dps { get; set; }
	/// <summary>
	/// healing per second
	/// </summary>
	public double Hps { get; set; }
	/// <summary>
	/// Id of the role played (dps/tank/heal)
	/// </summary>
	public byte RoleId { get; set; }
	/// <summary>
	/// Id of the spec played.
	/// </summary>
	public SpecId SpecId { get; set; }
	/// <summary>
	/// the ID of the in-game encounter.
	/// </summary>
	public EncounterId WowEncounterId { get; set; }
	/// <summary>
	/// recorded item level during the encounter
	/// </summary>
	public int ItemLevel { get; set; }

	/// <summary>
	/// Id of the EncounterInfoMetadata entity.
	/// </summary>
	public uint EncounterInfoMetadataId { get; set; }
	//this is fine to have lazy loaded.
	public virtual EncounterInfoMetadata? EncounterInfoMetadata { get; set; }

	/// <summary>
	/// Id of the PlayerMetadata entity
	/// </summary>
	public uint PlayerMetadataId { get; set; }
	public PlayerMetadata? PlayerMetadata { get; set; }
}