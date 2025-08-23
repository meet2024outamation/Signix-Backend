namespace SharedKernal.AuthorizeHandler
{
    public interface IPermissionService
    {
        Task<PermissionVM> GetPermissionsAsync();
    }
}
