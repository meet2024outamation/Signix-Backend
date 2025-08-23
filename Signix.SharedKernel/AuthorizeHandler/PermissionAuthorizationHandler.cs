using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernal.AuthorizeHandler
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.Identity is null || !context.User.Identity.IsAuthenticated) return;

            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            var userModulesPermissions = await permissionService.GetPermissionsAsync();
            if (userModulesPermissions.Moduels.Contains(requirement.Module) && userModulesPermissions.Permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
