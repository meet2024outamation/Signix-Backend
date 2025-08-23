using AutoMapper;
using Signix.IAM.API.Endpoints.Department;
using Signix.IAM.API.Models;
using Signix.IAM.API.Properties;
using Signix.IAM.API.Endpoints.Department;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.Context;
using Signix.IAM.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;
using SharedKernel.Services;
using System.Text;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class DepartmentService : ServiceBase, IDepartmentService
    {
        private readonly IManageUserService _manageUserService;
        private readonly IMapper _mapper;
        public DepartmentService(IAMDbContext _iamDBContext, IMapper mapper, IUser user, IManageUserService manageUserService) : base(_iamDBContext, user)
        {
            _mapper = mapper;
            _manageUserService = manageUserService;
        }

        public async Task<Result<string>> CreateDepartmentAsync(CreateDepartmentRequest request)
        {
            var departmentExist = await _iamDbConext.Departments.Where(dpt=> dpt.Name == request.DepartmentName && dpt.ClientId == _user.CurrentClientId).FirstOrDefaultAsync();

            if (departmentExist != null) {
                return Result<string>.Invalid(new List<ValidationError> { new() { Key = "Department", ErrorMessage = "Department with the Provided Name already exists" } });
            }
            var department = _mapper.Map<IAM.Entities.Department>(request);
            department.ClientId = _user.CurrentClientId;
            department.Id = Guid.NewGuid().ToString();
            department.CreatedById = _user.Id;
            department.CreatedDateTime = DateTimeOffset.Now;
            UserDepartment userDepartment = new UserDepartment();
            userDepartment.UserId = _user.Id;
            userDepartment.DepartmentId = department.Id;

            byte[] schemaBytes = Resource.Department_StandardSchema;
            string jsonSchema = Encoding.UTF8.GetString(schemaBytes);
            department.StandardSchema = jsonSchema;

            byte[] sampleJsonBytes = Resource.Department_SampleJson;
            string sampleJsonSchema = Encoding.UTF8.GetString(sampleJsonBytes);
            department.SampleJson = sampleJsonSchema;
            await _iamDbConext.Departments.AddAsync(department);
            await _iamDbConext.UserDepartments.AddAsync(userDepartment);
           await _iamDbConext.SaveChangesAsync();
            return Result<string>.Success(System.Text.Json.JsonSerializer.Serialize(new { Id = department.Id }));

        }

        public Task<Result<int>> DeleteDepartmentByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<string>> EditDepartmentAsync(UpdateDepartmentRequest request)
        {
            var department = await _iamDbConext.Departments.Where(dept => dept.Id == request.Id).FirstOrDefaultAsync();

            if (department == null) {
                return Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Department",ErrorMessage = "Department Doesn't Exist" } });
            }

            department = _mapper.Map(request.dept,department);
            department.ModifiedById = _user.Id;

            _iamDbConext.Departments.Update(department);
            await _iamDbConext.SaveChangesAsync(); 
            return Result<string>.Success(System.Text.Json.JsonSerializer.Serialize(new { Id = department.Id }));

        }

        public async Task<Result<GetDepartmentResponse>> GetDepartmentByIdAsync(string id)
        {
            var activeUsers = await _manageUserService.GetUsers(true,_user.CurrentClientId);
            if (activeUsers == null)
            {
                return Result<GetDepartmentResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Expression", ErrorMessage = "Users not found" } });
            }
            var departmentById = await _iamDbConext.Departments.Where(d=>d.Id == id && d.ClientId == _user.CurrentClientId).FirstOrDefaultAsync();

            if (departmentById == null)
            {
                return Result<GetDepartmentResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Department", ErrorMessage = "Department could not be found." } });
            }
            
            var department = _mapper.Map<GetDepartmentResponse>(departmentById);
            department.CreatedBy = activeUsers.SingleOrDefault(u => u.Id == departmentById.CreatedById)?.Name ?? "-";
            return Result<GetDepartmentResponse>.Success(department);
        }

        public async Task<Result<List<GetDepartmentResponse>>> GetDepartmentListAsync()
        {
            List<Department> departments = await _iamDbConext.Departments
                    .Where(d => d.ClientId == _user.CurrentClientId && d.IsActive)
                    .ToListAsync();
            var activeUsers = await _manageUserService.GetUsers(true, _user.CurrentClientId);

            if (activeUsers == null)
            {
                return Result<List<GetDepartmentResponse>>.Invalid(new List<ValidationError> { new ValidationError { Key = "Expression", ErrorMessage = "Users not found" } });
            }
            List<GetDepartmentResponse> getDepartmentResponses=new List<GetDepartmentResponse>();
          
            getDepartmentResponses = departments.OrderByDescending(d=>d.CreatedDateTime).Select(dt => new GetDepartmentResponse
            {
                Id = dt.Id,
                DepartmentName = dt.Name,
                Description = dt.Description,
                IsActive = dt.IsActive,
                CreatedBy = activeUsers.SingleOrDefault(u => u.Id == dt.CreatedById)?.Name ?? "-",
                CreatedDateTime = dt.CreatedDateTime.DateTime
            }).ToList();

            return Result<List<GetDepartmentResponse>>.Success(getDepartmentResponses);
        }

        public async Task<Result<List<GetDepartmentResponse>>> GetUserDepartmentListAsync(GetUserDepartmentR request)
        { 
            List<UserDepartment> departments;
            
            departments = await _iamDbConext.UserDepartments
                .Where(ud => ud.UserId == _user.Id && ud.Department.ClientId == _user.CurrentClientId)
                .Include(ud => ud.Department)
                .ToListAsync();
            
            if (request.IsActive.HasValue) {

                departments = departments.Where(ud => ud.Department.IsActive == request.IsActive).ToList();
            }
            var activeUsers = await _manageUserService.GetUsers(true, _user.CurrentClientId);

            if (activeUsers == null)
            {
                return Result<List<GetDepartmentResponse>>.Invalid(new List<ValidationError> { new ValidationError { Key = "Expression", ErrorMessage = "Users not found" } });
            }
            List<GetDepartmentResponse> getDepartmentResponses = new List<GetDepartmentResponse>();
            getDepartmentResponses = departments.Select(dt => new GetDepartmentResponse
            {
                Id = dt.DepartmentId,
                DepartmentName = dt.Department.Name,
                Description = dt.Department.Description,
                IsActive = dt.Department.IsActive,
                CreatedBy = activeUsers.SingleOrDefault(u => u.Id == dt.Department.CreatedById)?.Name ?? "-",
                CreatedDateTime = dt.Department.CreatedDateTime.DateTime
            }).OrderByDescending(c => c.CreatedDateTime).ToList();

            return Result<List<GetDepartmentResponse>>.Success(getDepartmentResponses);
        }

        public async Task<Result<string>> UpdateStatusByIdAsync(UpdateStatusRequest updateStatusRequest)
        {

            var department = await _iamDbConext.Departments.Where(dept => dept.Id == updateStatusRequest.Id).FirstOrDefaultAsync();

            if (department == null) {
                return Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Department" , ErrorMessage = "Department Doesn't Exist" } });
            }

            department.ModifiedById = _user.Id;
            department.IsActive = updateStatusRequest.IsActive;

            _iamDbConext.Update(department);
            await _iamDbConext.SaveChangesAsync();
            return Result<string>.Success(System.Text.Json.JsonSerializer.Serialize(new { Id = department.Id }));
        }
    }
}
