using Microsoft.AspNetCore.Http;
using SharedKernal.AuthorizeHandler;
using SharedKernal.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;


namespace SharedKernal.Models
{
    public class AppUser : IUser
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public int Id { get; private set; }

        public string? FirstName { get; private set; }

        public string? LastName { get; private set; }

        public string? Email { get; private set; }

        public string? CurrentTenantId { get; private set; }
        public string? CurrentTenantCode { get; private set; }

        public bool IsActive { get; private set; }
        public string? ConnectionString { get; private set; }
        public HashSet<string> Modules { get; private set; }
        public HashSet<string> Permissions { get; private set; }
        public bool? IsServicePrincipalUser { get; private set; }

        public AppUser(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            Permissions = new HashSet<string>();
            Modules = new HashSet<string>();

            if (httpContextAccessor.HttpContext == null || !httpContextAccessor.HttpContext.User.Identity!.IsAuthenticated)
                throw new UserNotFoundException();

            _httpClientFactory = httpClientFactory;
            UserWithTenants? userDetails = new();
            var client = _httpClientFactory.CreateClient(nameof(UrlsConfig.IAMUrl));
            int claimValue = GetClaimValueFromToken(httpContextAccessor.HttpContext.User.Claims);

            userDetails = client.GetFromJsonAsync<UserWithTenants?>($"api/user-detail").Result;

            if (userDetails is null) throw new UserNotFoundException();

            if (userDetails.Permissions is null) throw new PermissionsNotFoundException();

            if (userDetails != null)
            {
                Id = userDetails.Id;
                CurrentTenantId = userDetails.CurrentTenantId;
                CurrentTenantCode = userDetails.CurrentTenantCode;
                ConnectionString = userDetails.ConnectionString;
                FirstName = userDetails.FirstName;
                LastName = userDetails.LastName;
                Email = userDetails.Email;
                IsActive = userDetails.IsActive;
                IsServicePrincipalUser = claimValue == 1;
                Modules = userDetails.Modules;
                Permissions = userDetails.Permissions;
            }
        }
        private int GetClaimValueFromToken(IEnumerable<Claim> claims)
        {
            if (claims is null)
                throw new UserNotFoundException();

            var claim = claims.FirstOrDefault(c => c.Type.Equals("appidacr", StringComparison.OrdinalIgnoreCase)
            || c.Type.Equals("azpacr", StringComparison.OrdinalIgnoreCase));

            if (claim != null && int.TryParse(claim.Value, out int value))
            {
                return value;
            }
            throw new UserNotFoundException();
        }
    }

}
