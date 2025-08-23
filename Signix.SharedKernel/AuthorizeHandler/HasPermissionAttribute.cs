using Microsoft.AspNetCore.Authorization;

namespace SharedKernal.AuthorizeHandler
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permissions) : base(policy: permissions)
        {
        }
    }
}
