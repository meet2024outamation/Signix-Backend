using SharedKernel.Result;
using Signix.IAM.Entities;
using Signix.IAM.API.Endpoints.Client;
using Signix.IAM.API.Models;
using Signix.IAM.API.Endpoints.Client;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IClientServices
    {
        Task<Result<string>> CreateClientAsync(ClientCreateRequest ClientCR);
        Task<Result<string>> UpdateClientAsync(ClientEditRequest ClientER);
        Task<Result<ClientGetResponse>> GetClientByIdAsync(string id);
        Task<Result<bool>> CreateDatabaseAsync(string ClientId);
        Task<Result<bool>> SeedDataAsync(string ClientId);
        Task<Result<List<ClientGetResponse>>> GetClientAsync();
        Task<Result<int>> ChangeCurrentClientAsync(ChangeClientRequest request, string email);
        Task<Result<List<GetAccessibleClientResponse>>> GetAccessibleClientAsync(string email);
        Task<Result<string>> UpdateStatusByIdAsync(UpdateStatusByIdR request);
    }
}
