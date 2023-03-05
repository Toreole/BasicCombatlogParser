using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatlogParser.Data.Metadata
{
    public sealed class PerformanceMetadata
    {
        /// <summary>
        /// unique ID of the performance. autoincrement
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        /// <summary>
        /// GUID of the player
        /// </summary>
        public string PlayerGUID { get; set; } = "";
        public uint PlayerMetadataId { get; set; }
        public PlayerMetadata? PlayerMetadata { get; set; }
        /// <summary>
        /// encounterInfo UID
        /// </summary>
        public uint EncounterId { get; set; }
        public double Dps { get; set; }
        public double Hps { get; set; }
        public byte RoleId { get; set; }
        public byte SpecID { get; set; }

        public PerformanceMetadata(string playerGUID)
        {
            this.PlayerGUID = playerGUID;
        }
    }
}