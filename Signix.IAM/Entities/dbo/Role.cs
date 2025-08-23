using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;
[Index("Name", "ClientId", Name = "IDX_Roles_Name_ClientId")]
[Index("StatusId", Name = "IDX_Roles_StatusId")]
[Index("Name", "ClientId", Name = "Unique_Roles_Name_ClientId", IsUnique = true)]
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string Name { get; set; }

    [Required]
    [StringLength(36)]
    [Unicode(false)]
    public string ClientId { get; set; }

    public bool IsEditable { get; set; }

    public bool? IsServicePrincipal { get; set; }

    public int StatusId { get; set; }

    public int? ModifiedById { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("Roles")]
    public virtual Client Client { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [ForeignKey("StatusId")]
    [InverseProperty("Roles")]
    public virtual RoleStatus Status { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
