using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatlogParser.Data.Metadata
{
	public sealed class PlayerMetadata : EntityBase
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override uint Id { get; set; }
		public string GUID { get; set; } = "";
		public string Name { get; set; } = "";
		public string Realm { get; set; } = "";
		public ClassId ClassId { get; set; }

		//not strictly necessary, use a specialized query for this instead, using Take() before ToList()
		//public List<PerformanceMetadata> PerformanceMetadatas { get; set; } = new List<PerformanceMetadata>();

		public static PlayerMetadata From(PlayerInfo playerInfo)
		{
			return new PlayerMetadata()
			{
				GUID = playerInfo.GUID,
				Name = playerInfo.Name,
				Realm = playerInfo.Realm,
				ClassId = playerInfo.Class
			};
		}
	}
}