using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.Entities.Entities
{
    [Table("document_statuses")]
    public class DocumentStatus
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
