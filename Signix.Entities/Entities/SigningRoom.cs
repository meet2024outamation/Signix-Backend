using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Signix.Entities.Entities;

[Table("signing_rooms")]
public class SigningRoom
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
    public int? ClientId { get; set; }

    [Required]
    [Column("original_path")]
    public string OriginalPath { get; set; } = string.Empty;

    [Required]
    [Column("signed_path")]
    public string SignedPath { get; set; } = string.Empty;

    [Column("notary_id")]
    public int NotaryId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("modified_by")]
    public int ModifiedBy { get; set; }

    //[Column("status_id")]
    //public int StatusId { get; set; }

    [Column("meta_data")]
    public Dictionary<string, object>? MetaData { get; set; }
    [Column("sign_tags")]
    public Dictionary<string, object>? SignTags { get; set; }

    // Navigation properties
    [ForeignKey("NotaryId")]
    public virtual User Notary { get; set; } = null!;
    [ForeignKey("ClientId")]
    [JsonIgnore]
    public virtual Client? Client { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Signer> Signers { get; set; } = new List<Signer>();

    [JsonIgnore]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}