using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernal.AuthorizeHandler;
using System.ComponentModel.DataAnnotations;

namespace SharedKernal.Services;

public interface IUser
{
    public int Id { get; }//bind UserId or ServicePrincipalId
    public string? FirstName { get; }
    public string? LastName { get; }
    public string Name => $"{FirstName} {LastName}";

    public string? Email { get; }
    public string? CurrentTenantId { get; }
    public string? CurrentTenantCode { get; }
    public bool IsActive { get; }
    public string? ConnectionString { get; }
    public HashSet<string> Modules { get; }
    public HashSet<string> Permissions { get; }
    public bool? IsServicePrincipalUser { get; }
}



public class TokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenHandler> _logger;

    public TokenHandler(IHttpContextAccessor httpContextAccessor, ILogger<TokenHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        var token = ExtractToken();
        _logger.LogInformation(new { call = "Share AuthService:", Data = token, EmailAddress = _httpContextAccessor.HttpContext!.User.FindFirst("preferred_username")?.Value }.ToString());
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string ExtractToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            return httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        }
        return string.Empty;
    }
}

public class AuthService
{
    protected readonly IUser _user;
    public AuthService(IUser user)
    {
        this._user = user;
    }
}
public class UserBasicInfo
{
    public int Id { get; set; }
    public string? UniqueId { get; set; }
    public string Name => $"{FirstName} {LastName}";
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; }
    public string CurrentTenantId { get; set; } = null!;
}
public class UserWithTenants : UserBasicInfo
{
    public string ConnectionString { get; set; } = null!;
    public string? ServicePrincipalName { get; set; }
    public HashSet<string> Modules { get; set; } = null!;
    public HashSet<string> Permissions { get; set; } = null!;
    public string CurrentTenantCode { get; set; } = null!;

}

public class UrlsConfig
{
    public string IAMUrl { get; set; } = string.Empty;
}

public class TenantInfo
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string OfficeCode { get; set; } = null!;

    public int? TenantTypeId { get; set; }
    public string? TenantTypeName { get; set; }

    public string? Note { get; set; }

    public string? PhysicalLineText { get; set; }

    public string? PhysicalAdditionalLineText { get; set; }

    public string? PhysicalCityName { get; set; }

    public string? PhysicalStateCode { get; set; }


    public string? PhysicalPostalCode { get; set; }

    public string? MailingLineText { get; set; }

    public string? MailingAdditionalLineText { get; set; }

    public string? MailingCityName { get; set; }

    public string? MailingStateCode { get; set; }


    public string? MailingPostalCode { get; set; }
    public string? PreparedByPhone { get; set; }
    public string? PreparedByIndividualName { get; set; }

    public int? VendorTypeId { get; set; }

    public string? VendorName { get; set; }

    public string? VendorTaxpayerIdentifierValue { get; set; }

    public string? InitialAdminFirstName { get; set; }

    public string? InitialAdminMiddleName { get; set; }

    public string? InitialAdminLastname { get; set; }

    public string? InitialAdminEmail { get; set; }

    public string DatabaseCreationSource { get; set; } = null!;
    public string Status { get; set; } = null!;

    public DateTimeOffset CreatedDateTime { get; set; }
    public int ActiveUsers { get; set; }
    public bool IsActive { get; set; }
}
