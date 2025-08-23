using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

[Index("UserId", "ClientId", Name = "UQ_UserClients_UserId_ClientId", IsUnique = true)]
public class UserClient
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(36)]
    [Unicode(false)]
    public string ClientId { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("UserClients")]
    public virtual Client Client { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserClients")]
    public virtual User User { get; set; }
}
