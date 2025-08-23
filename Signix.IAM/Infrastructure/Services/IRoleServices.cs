using SharedKernel.Result;
using Signix.IAM.API.Endpoints.Role;
using Signix.IAM.API.Endpoints.Role;
using Signix.IAM.API.Models;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IRoleServices
    {
        Task<Result<List<GetRoleResponse>>> GetRoleAsync();
        Task<Result<GetRoleResponse>> GetRoleByIdAsync(int Id);
        Task<Result<List<GetRoleUserResponse>>> GetRoleUsersAsync();
        Task<Result<GetRoleResponse>> CreateAsync(RoleCreateRequest role);
        Task<Result<GetRoleResponse>> UpdateAsync(RoleUpdateRequest role);
        Task<Result<ErrorMessageVM>> UpdateStatusByIdAsync(RoleStatusRequest request);
    }
}
