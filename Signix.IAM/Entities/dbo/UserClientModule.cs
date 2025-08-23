using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

[PrimaryKey("ModuleId", "UserId", "ClientId")]
[Index("UserId", Name = "IDX_UserClientModules_UserId")]
[Index("ModuleId", Name = "IX_UserClientModules_ModuleId")]
public class UserClientModule
{
    [Key]
    public int UserId { get; set; }

    [Key]
    public int ModuleId { get; set; }

    public int? ModifiedById { get; set; }

    [Key]
    [StringLength(36)]
    [Unicode(false)]
    public string ClientId { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("UserClientModules")]
    public virtual Client Client { get; set; }

    [ForeignKey("ModuleId")]
    [InverseProperty("UserClientModules")]
    public virtual Module Module { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserClientModules")]
    public virtual User User { get; set; }
}
