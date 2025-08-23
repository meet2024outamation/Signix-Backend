using Signix.IAM.API.Models;

namespace Signix.IAM.API.Endpoints.Role
{
    public class RoleCreateRequest
    {
        public string Name { get; set; } = null!;
        public List<int> PermissionIds { get; set; }
        public bool IsEditable { get; set; }

        public bool IsActive { get; set; }

        public RoleCreateRequest()
        {
            PermissionIds = new List<int>();
        }
    }
}
