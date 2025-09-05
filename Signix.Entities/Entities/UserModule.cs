#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Signix.Entities.Entities;

[Index("ModifiedById", Name = "IDX_UserModules_ModifiedById")]
[Index("UserId", Name = "IDX_UserModules_UserId")]
[Index("ModuleId", Name = "IX_UserModules_ModuleId")]
[Index("ModuleId", "UserId", Name = "UK_UserModules", IsUnique = true)]
public partial class UserModule
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ModuleId { get; set; }

    public int? ModifiedById { get; set; }

    [ForeignKey("ModuleId")]
    [InverseProperty("UserModules")]
    public virtual Module Module { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserModules")]
    public virtual User User { get; set; } = null!;
}