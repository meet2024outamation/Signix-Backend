
using Signix.IAM.API.Endpoints.Department;
using Signix.IAM.API.Models;
using Signix.IAM.API.Endpoints.Department;
using SharedKernel.Result;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IDepartmentService
    {

        Task<Result<string>> CreateDepartmentAsync(CreateDepartmentRequest request);
        Task<Result<GetDepartmentResponse>> GetDepartmentByIdAsync(string id);

        Task<Result<List<GetDepartmentResponse>>> GetDepartmentListAsync();

        Task<Result<string>> EditDepartmentAsync(UpdateDepartmentRequest request);
        Task<Result<int>> DeleteDepartmentByIdAsync(string id);

        Task<Result<string>> UpdateStatusByIdAsync(UpdateStatusRequest updateStatusRequest);

        Task<Result<List<GetDepartmentResponse>>> GetUserDepartmentListAsync(GetUserDepartmentR request);
    }
}
