using SharedKernel.Result;
using Signix.IAM.API.Endpoints.UserPermission;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IUserPermissionServices
    {
        Task<Result<int>> PostUserModule(UpdateUserModuleRequest request);
        Task<Result<int>> DeleteUserModule(UpdateUserModuleRequest request);
    }
}
