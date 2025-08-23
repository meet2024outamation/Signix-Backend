using AutoMapper;
using SharedKernel.Result;
using Signix.IAM.API.Endpoints.UserPermission;
using Signix.IAM.Context;
using SharedKernel.Services;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class UserPermissionServices : ServiceBase, IUserPermissionServices
    {
        private readonly IMapper _mapper;
        public UserPermissionServices(IAMDbContext iamDbConext, IMapper mapper, IUser user):base(iamDbConext,user)
        {
            _mapper = mapper;
        }

        public async Task<Result<int>> DeleteUserModule(UpdateUserModuleRequest request)
        {
            var userModule = await _iamDbConext.UserClientModules.FindAsync(request.ModuleId, request.UserId, request.ClientId);
            if (userModule is not null)
            {
                _iamDbConext.UserClientModules.Remove(userModule);
                return await _iamDbConext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<Result<int>> PostUserModule(UpdateUserModuleRequest request)
        {
            var user = await _iamDbConext.Users.FindAsync(request.UserId);
            var module = await _iamDbConext.Modules.FindAsync(request.ModuleId);
            var errors = new List<ValidationError>();
            if (module is null)
            {
                errors.Add(new ValidationError { Key = "UserModule", ErrorMessage = "Module not found" });
            }
            if (user is null)
            {
                errors.Add(new ValidationError { Key = "UserModule", ErrorMessage = "User not found" });
            }
            if (errors.Count > 0)
            {
                return Result<int>.Invalid(errors);
            }
            var userModule = await _iamDbConext.UserClientModules.FindAsync(request.ModuleId, request.UserId, request.ClientId);
            if (userModule is not null)
            {
                return Result<int>.Success(0);
            }
            var newUserModule = _mapper.Map<Entities.UserClientModule>(request);
            newUserModule.ModifiedById = _user.Id;
            await _iamDbConext.UserClientModules.AddAsync(newUserModule);
            return await _iamDbConext.SaveChangesAsync();
        }
    }
}
