using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Signix.Entities.Entities
{
    [Table("signers")]
    public class Signer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; } // Added primary key

        [Column("signing_room_id")]
        public int SigningRoomId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("designation_id")]
        public int DesignationId { get; set; }

        /// <summary>
        /// Base64 encoded signature data
        /// </summary>
        [Column("signature_data")]
        public string? SignatureData { get; set; }
        [Column("sined_at")]
        public DateTime? SignedAt { get; set; }

        [ForeignKey("SigningRoomId")]
        [JsonIgnore]
        public virtual SigningRoom SigningRoom { get; set; } = null!;

        [ForeignKey("DesignationId")]
        [JsonIgnore]
        public virtual Designation Designation { get; set; } = null!;
    }
}
