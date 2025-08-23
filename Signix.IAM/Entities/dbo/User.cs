using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string UniqueId { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string UserName { get; set; }

    [Required]
    [StringLength(255)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string FirstName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string MiddleName { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string LastName { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string PhoneNumber { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string TimeZone { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string CompanyName { get; set; }

    public string Data { get; set; }

    [Column("AzureADUserId")]
    [StringLength(128)]
    public string AzureAduserId { get; set; }

    [StringLength(36)]
    [Unicode(false)]
    public string CurrentClientId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public bool? IsServicePrincipal { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public int? CreatedById { get; set; }

    public int? ModifiedById { get; set; }

    [ForeignKey("CreatedById")]
    [InverseProperty("InverseCreatedBy")]
    public virtual User CreatedBy { get; set; }

    [ForeignKey("CurrentClientId")]
    [InverseProperty("Users")]
    public virtual Client CurrentClient { get; set; }

    [InverseProperty("CreatedBy")]
    public virtual ICollection<User> InverseCreatedBy { get; set; } = new List<User>();

    [InverseProperty("ModifiedBy")]
    public virtual ICollection<User> InverseModifiedBy { get; set; } = new List<User>();

    //[InverseProperty("User")]
    //public virtual ICollection<Jwttoken> Jwttokens { get; set; } = new List<Jwttoken>();

    [ForeignKey("ModifiedById")]
    [InverseProperty("InverseModifiedBy")]
    public virtual User ModifiedBy { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserClientModule> UserClientModules { get; set; } = new List<UserClientModule>();

    [InverseProperty("User")]
    public virtual ICollection<UserClient> UserClients { get; set; } = new List<UserClient>();

    //[InverseProperty("User")]
    //public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
