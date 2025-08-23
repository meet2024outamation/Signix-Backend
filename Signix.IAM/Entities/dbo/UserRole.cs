using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

[PrimaryKey("UserId", "RoleId")]
[Index("UserId", Name = "IDX_UserRoles_UserId")]
[Index("RoleId", Name = "IX_UserRoles_RoleId")]
public class UserRole
{
    [Key]
    public int UserId { get; set; }

    [Key]
    public int RoleId { get; set; }

    public int? ModifiedById { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("UserRoles")]
    public virtual Role Role { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserRoles")]
    public virtual User User { get; set; }
}
