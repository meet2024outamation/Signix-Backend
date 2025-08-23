using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SharedKernal.AuthorizeHandler;
using SharedKernal.Models;
using SharedKernel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Models
{
    public class HMACAppUser : IHMACUser
    {
        public int Id { get; private set; }       
        public string? CurrentTenantId { get; private set; }
        public string? CurrentTenantCode { get; private set; }
        public bool IsActive { get; private set; }
        public string? ConnectionString { get; private set; }


        public HMACAppUser(IHttpContextAccessor httpContextAccessor, string secretKey, string headerName,string Endpoints,string requestBody = "")
        {            
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new UserNotFoundException();
            }

            var receivedEndpoint = httpContext.Request.Path.Value;

            var webhookConfig = Endpoints.Equals(receivedEndpoint, StringComparison.OrdinalIgnoreCase);
            if (!webhookConfig)
            {               
                throw new UserNotFoundException();
            }
            var receivedHmac = httpContext.Request.Headers[headerName].ToString();
            if (string.IsNullOrEmpty(receivedHmac))
            {
                throw new UserNotFoundException();
            }

            var sharedSecret = secretKey;

            var generatedHmac = ComputeHmacSha256(requestBody, sharedSecret);

            if (receivedHmac != generatedHmac)
            {
                throw new UserNotFoundException();
            }
        }

        private string ComputeHmacSha256(string data, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

}
