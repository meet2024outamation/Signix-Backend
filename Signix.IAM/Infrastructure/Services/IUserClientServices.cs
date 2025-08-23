using SharedKernel.Result;
using Signix.IAM.API.Endpoints.Client;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IUserClientServices
    {
        Task UpdateUserClientAsync(string clientId, int userId);

    }
}
