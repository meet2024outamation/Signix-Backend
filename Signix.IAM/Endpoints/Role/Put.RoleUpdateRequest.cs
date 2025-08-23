using Signix.IAM.API.Models;

namespace Signix.IAM.API.Endpoints.Role
{
    public class RoleUpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int StatusId { get;  set; }
        public bool IsEditable { get;  set; }
        public List<int> PermissionIds { get; set; }

        public RoleUpdateRequest()
        {
            PermissionIds = new List<int>();
        }
    }
}
