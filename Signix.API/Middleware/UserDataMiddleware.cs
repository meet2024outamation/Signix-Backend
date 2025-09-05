using SharedKernel.Services;
using SharedKernel.Utility;
using System.Security.Claims;

namespace Signix.API.Middleware;

public class UserDataMiddleware
{
    private readonly RequestDelegate _next;

    public UserDataMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            // Determine if the user is a service principal
            bool isServicePrincipalUser = Utility.IsServicePrincipalUser(user.Claims);


            // Get the user identifier from claims
            string? userId = isServicePrincipalUser
                ? user.FindFirstValue("azp")
                : user.FindFirstValue("preferred_username")?.Replace("live.com#", "");

            if (!string.IsNullOrEmpty(userId))
            {
                // Fetch user details from cache service
                //var userDetails = await cacheService.GetUserById(userId, requestTenantId, isServicePrincipalUser);
                //if (userDetails != null)
                //{
                context.Items["UserDetails"] = new UserDetail
                {
                    Email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? "",
                    FirstName = user.FindFirstValue(ClaimTypes.GivenName) ?? user.FindFirstValue("given_name") ?? "",
                    LastName = user.FindFirstValue(ClaimTypes.Surname) ?? user.FindFirstValue("family_name") ?? "",
                    IsServicePrincipal = isServicePrincipalUser,
                    Id = 1
                };
                //}
            }
        }
        await _next(context);
    }
}