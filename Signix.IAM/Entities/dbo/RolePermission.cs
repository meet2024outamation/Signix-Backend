using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

[PrimaryKey("PermissionId", "RoleId")]
[Index("RoleId", Name = "IDX_RolePermissions_RoleId")]
[Index("PermissionId", Name = "IX_RolePermissions_PermissionId")]
public class RolePermission
{
    [Key]
    public int RoleId { get; set; }

    [Key]
    public int PermissionId { get; set; }

    public int? ModifiedById { get; set; }

    [ForeignKey("PermissionId")]
    [InverseProperty("RolePermissions")]
    public virtual Permission Permission { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("RolePermissions")]
    public virtual Role Role { get; set; }
}
