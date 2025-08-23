using SharedKernel.Result;
using Signix.IAM.API.Endpoints.Permission;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IPermissionServices
    {
        Task<Result<List<GetPermissionResponse>>> GetPermissionAsync();
        Task<Result<GetPermissionResponse>> GetPermissionByIdAsync(int id);
    }
}
