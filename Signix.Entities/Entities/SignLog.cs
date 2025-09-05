using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Signix.Entities.Entities;

public class SignLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("document_id")]
    public int DocumentId { get; set; }
    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;
    [ForeignKey("DocumentId")]
    [JsonIgnore]
    public virtual Document Document { get; set; } = null!;
}
