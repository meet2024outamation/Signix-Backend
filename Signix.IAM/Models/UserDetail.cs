using SharedKernal.Services;

namespace Signix.IAM.API.Models
{
    public class UserDetail : UserBasicInfo
    {
        public IList<AccessibleClientDTO> AccessibleClient { get; set; } = null!;
        public IList<UserRoleDTO> UserRoles { get; set; } = null!;
        public IList<ModuleDTO> Modules { get; set; } = null!;
        //public IList<DepartmentDTO> Departments { get; set; } = null!;

        public UserDetail()
        {
            UserRoles = new List<UserRoleDTO>();
            AccessibleClient = new List<AccessibleClientDTO>();
            Modules = new List<ModuleDTO>();
            //Departments = new List<DepartmentDTO>();
        }
    }

    public class DepartmentDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class AccessibleClientDTO
    {
        public string ClientId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsCurrentClient { get; set; }
        public string? ClientType { get; set; }
        public int? ClientTypeId { get; set; }
    }

    public class PermissionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = null!;
        public bool IsServicePrincipal { get; set; }
    }

    public class UserRoleDTO
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string RoleName { get; set; } = null!;
        public IList<PermissionDTO> Permissions { get; set; } = null!;
        public UserRoleDTO()
        {
            Permissions = new List<PermissionDTO>();
        }
    }

    public class ModuleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int StatusId { get; internal set; }
        public bool IsEditable { get; internal set; }
        public bool IsServicePrincipal { get; internal set; }
        public List<int> PermissionIds { get; set; }

        public RoleDTO()
        {
            PermissionIds = new List<int>();
        }
    }
    public class RoleDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public bool IsEditable { get; set; }
        public bool IsServicePrincipal { get; internal set; }
        public int StatusId { get; set; }
        public string RoleStatus { get; set; } = null!;
        public int NumberOfUsers { get; set; }
        public List<PermissionDTO> Permissions { get; set; }

        public RoleDetailDTO()
        {
            Permissions = new List<PermissionDTO>();
        }
    }
    public class RolePermissionDTO
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public int? ModifiedById { get; set; }

        public PermissionDTO Permission { get; set; } = null!;
        public RoleDTO Role { get; set; } = null!;
    }
}
