using SharedKernel.Services;
using Signix.IAM.API.Infrastructure.Services;
using SharedKernel.AuthorizeHandler;
using Signix.IAM.Context;
using Azure.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Signix.IAM.API.Infrastructure.Utility
{
    public class AppAMUser : IUser
    {
        private readonly IAMDbContext _iAMDbContext;

        private readonly IMemCacheService _cacheService;

        public int Id { get; private set; }

        public string? FirstName { get; private set; }

        public string? LastName { get; private set; }

        public string? Email { get; private set; }

        public string? CurrentClientId { get; private set; }

        public string? CurrentClientCode { get; private set; }

        public bool IsActive { get; private set; }
        public string? ConnectionString { get; private set; }

        public HashSet<string> Modules { get; private set; }

        public HashSet<string> Permissions { get; private set; }
        public HashSet<string> DepartmentIds { get; private set; }
        public List<DepartmentObj> DepartmentNames { get; private set; }
        public bool? IsServicePrincipalUser { get; private set; }

        public string CurrentClientName { get; set; }

        public AppAMUser(IAMDbContext iAMDbContext, IHttpContextAccessor httpContextAccessor, IMemCacheService cacheService)
        {
            _iAMDbContext = iAMDbContext;
            Permissions = new HashSet<string>();
            Modules = new HashSet<string>();
            UserWithClients? userDetails = new();
            var user = httpContextAccessor.HttpContext!.User;
            _cacheService = cacheService;

            if (!user.Identity!.IsAuthenticated) throw new UserNotFoundException();

            int claimValue = GetClaimValueFromToken(httpContextAccessor.HttpContext.User.Claims);

            bool isServicePrincipalUser = claimValue == 1;
            var clientId = httpContextAccessor.HttpContext!.Request.Headers["Client-Id"].ToString();

            userDetails = _cacheService.GetUserById(user.FindFirstValue(GetUniqueIdentityParameter(httpContextAccessor.HttpContext.User.Claims))!, clientId).Result;

            if (userDetails is null) throw new UserNotFoundException();

            if (!userDetails.IsActive)
            {
                throw new UserInActiveException();
            }

           // if (!httpContextAccessor.HttpContext.Request.Path.Equals("/api/user/login", StringComparison.OrdinalIgnoreCase) &&
           //!httpContextAccessor.HttpContext.Request.Path.Equals("/api/user/logout", StringComparison.OrdinalIgnoreCase) && 
           //!isServicePrincipalUser)
           // {
           //     string tokenSignature = Utility.ParsedToken(httpContextAccessor.HttpContext.Request.Headers["Authorization"].First()!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]).Signature;

           //     // Fetch the token from the database based on the user ID
           //     var userToken = _iAMDbContext.Jwttokens.FirstOrDefault(u => u.UserId == userDetails.Id && u.Token == tokenSignature);

           //     // If userToken and userid is null, throw exception
           //     if (userToken == null) throw new UserNotFoundException();
           // }

            if (userDetails != null)
            {
                Id = userDetails.Id;
                FirstName = userDetails.FirstName;
                LastName = userDetails.LastName;
                CurrentClientId = userDetails.CurrentClientId;
               // CurrentClientCode = userDetails.CurrentClientCode;
                IsActive = userDetails.IsActive;
               // ConnectionString = userDetails.ConnectionString;
                Email = userDetails.Email;
                Modules = userDetails.Modules;
                Permissions = userDetails.Permissions;
                IsServicePrincipalUser = isServicePrincipalUser;
                CurrentClientName = userDetails.CurrentClientName;
                DepartmentIds = userDetails.DepartmentIds;
                DepartmentNames = userDetails.DepartmentNames;
            }
        }
        public static int GetClaimValueFromToken(IEnumerable<Claim> claims)
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

        public static string GetUniqueIdentityParameter(IEnumerable<Claim> claims)
        {
            int claimValue = GetClaimValueFromToken(claims);
            return claimValue == 1 ? "azp" : "preferred_username";
        } 
    }

}
