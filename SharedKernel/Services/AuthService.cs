﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.AuthorizeHandler;
using SharedKernel.Models;
using System.Text.Json.Serialization;

namespace SharedKernel.Services
{
    public interface IUser
    {
        public int Id { get; }
        public string? FirstName { get; }
        public string? LastName { get; }
        public string Name => $"{FirstName} {LastName}";
        public string? Email { get; }
        public HashSet<string> Modules { get; }
        public HashSet<string> Permissions { get; }
        public string? UserStatus { get; }
        public bool IsServicePrincipal { get; }
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

            var headersInfo = ExtractTokenAndHeaders();
            if (headersInfo != null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", headersInfo.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private HeadersInfo? ExtractTokenAndHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                return new HeadersInfo
                {
                    Token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""),
                };
            }
            return null;
        }
    }

    public class ServicePrincipalHttpClientHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;
        private readonly AzureADConfig _azureADConfig;
        public ServicePrincipalHttpClientHandler(ITokenService tokenService, IOptions<AzureADConfig> azureADConfig)
        {
            _tokenService = tokenService;
            _azureADConfig = azureADConfig.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string token = await _tokenService.GetAccessTokenAsync(new List<string> { $"api://{_azureADConfig.ClientId}/.default" });

            // Add the token to the request headers
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Call the base class implementation
            return await base.SendAsync(request, cancellationToken);
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
        public string UserStatus { get; set; } = null!;
        public bool IsServicePrincipal { get; set; }
        public DateTimeOffset? LastLoginDateTime { get; set; }
    }
    public class UserDetail : UserBasicInfo
    {
        [JsonIgnore]
        public HashSet<string> Modules { get; set; } = null!;
        public HashSet<string> Permissions { get; set; } = null!;

    }

   
    public class HeadersInfo
    {
        public string? Token { get; set; }

    }
}
