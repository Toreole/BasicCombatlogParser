using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatlogParser.Data.Metadata
{
    public sealed class PlayerMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        public string GUID { get; set; } = "";
        public string Name { get; set; } = "";
        public string Realm { get; set; } = "";
        public byte ClassID { get; set; }
    }
}