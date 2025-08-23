using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

public class ClientStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; }

    [StringLength(900)]
    [Unicode(false)]
    public string Description { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Status")]
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
