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

        [MaxLength(100)]
        [Column("designation")]
        public string? Designation { get; set; }

        [Column("signature_path")]
        public string? SignaturePath { get; set; }

        [Column("signed_at")]
        public DateTime? SignedAt { get; set; } // Added for tracking when signed

        [Column("signing_order")]
        public int? SigningOrder { get; set; } // Added for signing sequence

        // Navigation properties
        [ForeignKey("SigningRoomId")]
        [JsonIgnore]
        public virtual SigningRoom SigningRoom { get; set; } = null!;
    }
}
