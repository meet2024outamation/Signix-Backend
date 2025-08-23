using SharedKernal.Services;

namespace SharedKernal.AuthorizeHandler;

public class PermissionService : AuthService, IPermissionService
{
    
    public PermissionService(IUser user):base(user)
    {
    }

    public async Task<PermissionVM> GetPermissionsAsync()
    {
        return new PermissionVM
        {
            Moduels = _user.Modules,
            Permissions = _user.Permissions,
        };
    }
}
