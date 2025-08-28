using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Signix.Entities.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("meta_data", TypeName = "jsonb")]
        public JsonElement MetaData { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? Type { get; set; }

        [Column("AzureADUserId")]
        [StringLength(128)]
        public string? AzureAduserId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<SigningRoom> SigningRooms { get; set; } = new List<SigningRoom>();
    }
}
