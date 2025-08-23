using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

[Index("Code", Name = "IDX_Modules_Code", IsUnique = true)]
[Index("Name", Name = "IDX_Modules_Name", IsUnique = true)]
public class Module
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(900)]
    public string Name { get; set; }

    [Required]
    [StringLength(900)]
    public string Code { get; set; }

    [InverseProperty("Module")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [InverseProperty("Module")]
    public virtual ICollection<UserClientModule> UserClientModules { get; set; } = new List<UserClientModule>();
}
