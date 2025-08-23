using Signix.IAM.API.Endpoints.Client;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel.Models;
using SharedKernel.Result;
using SharedKernel.Services;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IMemCacheService
    {
        EmailTemplate? InviteUser { get; }
        EmailTemplate? ClientAssignment { get; }
        Task<UserWithClients> GetUserById(string uniqueId, string clientId);
        Task<Result<List<GetAccessibleClientResponse>>> GetAccessibleClientAsync(string email);
        Task RemoveCache(string key);
    }
}
