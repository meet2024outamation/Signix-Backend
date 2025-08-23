using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Signix.IAM.Entities.dbo;

[Index("ClientTypeId", Name = "IDX_Clients_ClientTypeId")]
[Index("Identifier", Name = "IDX_Clients_Identifier")]
[Index("Code", Name = "UQ_Clients_Code", IsUnique = true)]
[Index("Name", Name = "UQ_Clients_Name", IsUnique = true)]
public class Client
{
    [Key]
    [StringLength(36)]
    [Unicode(false)]
    public string Id { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string Identifier { get; set; }

    [Required]
    [StringLength(128)]
    [Unicode(false)]
    public string Code { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string Address { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string ZipCode { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string PhoneNumber { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; }

    [Required]
    [StringLength(2)]
    [Unicode(false)]
    public string State { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string City { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string CompanyName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string OfficeCode { get; set; }

    [StringLength(450)]
    public string ConnectionString { get; set; }

    public int? ClientTypeId { get; set; }

    public string Note { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PhysicalLineText { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PhysicalAdditionalLineText { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PhysicalCityName { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string PhysicalStateCode { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string PhysicalPostalCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MailingLineText { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MailingAdditionalLineText { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MailingCityName { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string MailingStateCode { get; set; }

    [StringLength(13)]
    [Unicode(false)]
    public string MailingPostalCode { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string PreparedByPhone { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string RecordReturnToPhone { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string PreparedByIndividualName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string TempColumn { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string InitialAdminFirstName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string InitialAdminMiddleName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string InitialAdminLastname { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string InitialAdminEmail { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ContactPointTelephoneValue { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ContactPointFaxValue { get; set; }

    public int? StatusId { get; set; }

    public string StandardSchema { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public int CreatedById { get; set; }

    public int ModifiedById { get; set; }

    //[ForeignKey("ClientTypeId")]
    //[InverseProperty("Clients")]
    //public virtual ClientType ClientType { get; set; }

    //[InverseProperty("Client")]
    //public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    //[InverseProperty("Client")]
    //public virtual ICollection<Jwttoken> Jwttokens { get; set; } = new List<Jwttoken>();

    [InverseProperty("Client")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [ForeignKey("StatusId")]
    [InverseProperty("Clients")]
    public virtual ClientStatus Status { get; set; }

    //[InverseProperty("Client")]
    //public virtual ICollection<UserClientModule> UserClientModules { get; set; } = new List<UserClientModule>();

    [InverseProperty("Client")]
    public virtual ICollection<UserClient> UserClients { get; set; } = new List<UserClient>();

    [InverseProperty("CurrentClient")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
