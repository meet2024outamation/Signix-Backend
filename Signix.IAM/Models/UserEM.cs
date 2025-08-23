
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using SharedKernel.Services;

namespace Signix.IAM.API.Models
{
    public class UserEM : UserCM
    {
        public int? Id { get; set; }
        public string Name
        {
            get
            {
                return $"{this.LastName}, {this.FirstName}";
            }
        }
    }
    public class UserCM
    {
        public string? UniqueId { get; set; }

        [EmailAddress]
        public string Email { get; set; } = null!;
        
        public string UserName { get; set; }
        public string FirstName { get; set; } = null!;
        //public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        //public string? Suffix { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string? ClientIds { get; set; } = null!;
        public List<string> DepartmentIds { get; set; } = null!;
        //public List<string> DepartmentNames { get; set; } = null!;
        public string? CurrentClientName { get; set; } = null;
        public string? CurrentClientId { get; set; }
        public bool IsServicePrincipal {  get; set; }

        [StringLength(5)]
        [Unicode(false)]
        public string? TimeZone { get; set; }

        //[StringLength(200)]
        //[Unicode(false)]
        //public string? CompanyName { get; set; }

        public BarNotary? Data { get; set; }

        public List<int> RoleIds { get; set; }
        public UserCM()
        {
            RoleIds = new List<int>();
        }

    }

    public class UserVM : UserEM
    {
        public IList<UserRoleDTO> UserRoles { get; set; } = null!;
        public IList<ModuleDTO> Modules { get; set; } = null!;
        public UserVM()
        {
            UserRoles = new List<UserRoleDTO>();
            Modules = new List<ModuleDTO>();
        }
    }

    public class UserParam
    {
        public bool? IsActive { get; set; }
        public string? CurrentClientId { get; set; } = null!;
    }

    public class Bar
    {
        public string? Name { get; set; }
        public int? Number { get; set; }
    }

    public class Notary
    {
        public string? StateCode { get; set; }
        public int? County { get; set; }
        public DateOnly? CommissionExpiration { get; set; }
    }

    public class BarNotary
    {
        public List<Bar>? Bars { get; set; }
        public List<Notary>? Notaries { get; set; }
    }

    public class UserList : UserBasicInfo
    {
        public string? PhoneNumber { get; set; }
        public string? CurrentClientName { get; set; }
        public List<string>? AccessibleClientNames { get; set; } = new List<string>();
        public List<int> RoleIds { get; set; } = new List<int>();
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
