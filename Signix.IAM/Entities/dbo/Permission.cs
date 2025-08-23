using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Signix.IAM.Entities.dbo;

[Index("Code", Name = "IDX_Permissions_Code", IsUnique = true)]
[Index("ModuleId", Name = "IDX_Permissions_ModuleId")]
[Index("Name", Name = "IDX_Permissions_Name", IsUnique = true)]
public class Permission
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(900)]
    public string Name { get; set; }

    [Required]
    [StringLength(900)]
    public string Code { get; set; }

    public bool? IsServicePrincipal { get; set; }

    public int ModuleId { get; set; }

    [ForeignKey("ModuleId")]
    [InverseProperty("Permissions")]
    public virtual Module Module { get; set; }

    [InverseProperty("Permission")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
