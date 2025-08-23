using Microsoft.AspNetCore.Authorization;

namespace SharedKernal.AuthorizeHandler
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
        public string Module
        {
            get
            {
                if (Permission.IsNullOrEmpty()) return string.Empty;
                return Permission.Split('.')[0];
            }
        }
    }
}
