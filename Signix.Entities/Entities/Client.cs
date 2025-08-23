using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Signix.Entities.Entities;

[Table("clients")]
public class Client
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

    [Required]
    [Column("azure_client_id")]
    public string AzureClientId { get; set; } = string.Empty;

    [Column("client_secret", TypeName = "jsonb")]
    public JsonElement ClientSecret { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("modified_by")]
    public int ModifiedBy { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
