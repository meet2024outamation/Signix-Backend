using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace SharedKernal.AuthorizeHandler
{
    public static class AuthorizeStartUp
    {
       public static void AuthorizeStartUpConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
            builder.Services.AddTransient<IPermissionService, PermissionService>();
        }
    }
}
