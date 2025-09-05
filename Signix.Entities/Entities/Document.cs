using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Signix.Entities.Entities
{
    [Table("documents")]
    public class Document
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        //[Column("client_id")]
        //public int ClientId { get; set; }

        [Column("file_size")]
        public long FileSize { get; set; }

        [MaxLength(50)]
        [Column("file_type")]
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Document tags stored as JSONB for flexible key-value pairs
        /// Example: {"borr_name": "John Doe", "loan_amount": 50000, "credit_score": 750}
        /// </summary>
        [Required]
        [Column("doc_tags")]
        public Dictionary<string, object> DocTags { get; set; } = new();

        [Column("signing_room_id")]
        public int SigningRoomId { get; set; }

        [Column("document_status_id")]
        public int DocumentStatusId { get; set; }

        // Navigation properties
        [ForeignKey("DocumentStatusId")]
        public virtual DocumentStatus DocumentStatus { get; set; } = null!;

        [ForeignKey("SigningRoomId")]
        [JsonIgnore]
        public virtual SigningRoom SigningRoom { get; set; } = null!;

        //[ForeignKey("ClientId")]
        //[JsonIgnore]
        //public virtual Client Client { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<SignLog> SignLogs { get; set; } = new List<SignLog>();
    }
}
