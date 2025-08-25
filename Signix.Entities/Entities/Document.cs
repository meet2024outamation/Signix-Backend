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

        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("file_size")]
        public long FileSize { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("file_type")]
        public string FileType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("doc_tags")]
        public string? DocTags { get; set; }

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

        [ForeignKey("ClientId")]
        [JsonIgnore]
        public virtual Client Client { get; set; } = null!;
    }
}
