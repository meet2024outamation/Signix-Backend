using AutoMapper;
using AutoMapper.QueryableExtensions;
using Signix.IAM.API.Endpoints.Role;
using Signix.IAM.API.Models;
using Signix.IAM.API.Endpoints.Department;
using Signix.IAM.API.Endpoints.Role;
using Signix.IAM.API.Models;
using Signix.IAM.Context;
using Signix.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;
using SharedKernel.Services;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class RoleServices : ServiceBase, IRoleServices
    {
        private readonly IMapper _mapper;
        public RoleServices(IAMDbContext iamDbConext, IMapper mapper, IUser user) : base(iamDbConext, user)
        {
            _mapper = mapper;

        }

        public async Task<Result<List<GetRoleResponse>>> GetRoleAsync()
        {
            var roles = await _iamDbConext.Roles.Include(r => r.RolePermissions).Where(r => r.IsServicePrincipal != true).ProjectTo<GetRoleResponse>(_mapper.ConfigurationProvider).ToListAsync();
            return Result<List<GetRoleResponse>>.Success(roles);
        }
        public async Task<Result<GetRoleResponse>> CreateAsync(RoleCreateRequest roleCreateRequest)
        {
            var existingRoleByName = await _iamDbConext.Roles.AnyAsync(s => s.Name == roleCreateRequest.Name && s.ClientId == _user.CurrentClientId);
            if (existingRoleByName)
            {
                return Result<GetRoleResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Role", ErrorMessage = "Role is already exists" } });
            }
            //var status = await _iamDbConext.RoleStatuses.SingleAsync(s => s.Name == "Active");
            var role = new Entities.Role
            {
                Name = roleCreateRequest.Name,
                ClientId = _user.CurrentClientId!,
                ModifiedById = _user.Id,
                IsEditable = roleCreateRequest.IsEditable,
                StatusId = roleCreateRequest.IsActive ? 1 : 2
            };
            await _iamDbConext.Roles.AddAsync(role);
            await _iamDbConext.SaveChangesAsync();

            var rolePermissions = roleCreateRequest.PermissionIds.Select(permissionId => new Entities.RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });

            await _iamDbConext.RolePermissions.AddRangeAsync(rolePermissions);
            await _iamDbConext.SaveChangesAsync();

            return _mapper.Map<GetRoleResponse>(await _iamDbConext.Roles.ProjectTo<GetRoleResponse>(_mapper.ConfigurationProvider).SingleAsync(r => r.Id == role.Id && r.ClientId == _user.CurrentClientId));
        }

        public async Task<Result<GetRoleResponse>> UpdateAsync(RoleUpdateRequest roleUpdateRequest)
        {
            Entities.Role? existingRole = await _iamDbConext.Roles.FindAsync(roleUpdateRequest.Id);
            if (existingRole == null)
            {
                return Result<GetRoleResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Role", ErrorMessage = "Role is not found" } });
            }
            var existingRoleByName = await _iamDbConext.Roles.AnyAsync(s => s.Name == roleUpdateRequest.Name && s.Id != roleUpdateRequest.Id);
            if (existingRoleByName)
            {
                return Result<GetRoleResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Role", ErrorMessage = "Role is already exists" } });
            }

            _iamDbConext.RolePermissions.RemoveRange(_iamDbConext.RolePermissions.Where(rp => rp.RoleId == existingRole.Id));

            var rolePermissions = roleUpdateRequest.PermissionIds.Select(permissionId => new Entities.RolePermission
            {
                RoleId = existingRole.Id,
                PermissionId = permissionId
            });
            // Need to Add Code to Update the UserModule Aswell

            // Step .1 Get All the Users Who Have This Role
            var userWithCurrentRole = await _iamDbConext.UserRoles.Where(userRoles => userRoles.RoleId == existingRole.Id).Select(userRoles => userRoles.UserId).ToListAsync();
            // Once we got all the user with the role in the context we will update there UserModules
            List<UserClientModule>? updateduserModules = new List<UserClientModule>();
            foreach (var userId in userWithCurrentRole)
            {
                updateduserModules.AddRange(await _iamDbConext.RolePermissions
                    .Where(r => r.RoleId == existingRole.Id)
                    .Select(p => p.Permission.ModuleId)
                    .Distinct()
                    .Select(p => new UserClientModule
                    {
                        ModuleId = p,
                        UserId = userId,
                        ClientId = _user.CurrentClientId!,
                        ModifiedById = _user.Id
                    })
                    .ToListAsync());
            }
            // First We Have to Filter The List Of User Modules
            var userModules = await _iamDbConext.UserClientModules.ToListAsync();
            var existingUserModules = userModules.Where(um => updateduserModules.Any(up => up.UserId == um.UserId && up.ModuleId == um.ModuleId)).ToList();
            
            _iamDbConext.RemoveRange(existingUserModules);
            // Update The User Module Table
            await _iamDbConext.UserClientModules.AddRangeAsync(updateduserModules);
            existingRole.StatusId = roleUpdateRequest.StatusId;
            existingRole.Name = roleUpdateRequest.Name;
            existingRole.IsEditable = roleUpdateRequest.IsEditable;
            existingRole.ModifiedById = _user.Id;
            _iamDbConext.Roles.Update(existingRole);
            await _iamDbConext.RolePermissions.AddRangeAsync(rolePermissions);

            await _iamDbConext.SaveChangesAsync();

            return _mapper.Map<GetRoleResponse>(await _iamDbConext.Roles.ProjectTo<GetRoleResponse>(_mapper.ConfigurationProvider).SingleAsync(r => r.Id == existingRole.Id));
        }

        public async Task<Result<GetRoleResponse>> GetRoleByIdAsync(int id)
        {
            var existingRole = await _iamDbConext.Roles.Where(x => x.IsServicePrincipal != true).ProjectTo<GetRoleResponse>(_mapper.ConfigurationProvider).SingleOrDefaultAsync(s => s.Id == id);
            if (existingRole == null)
            {
                return Result<GetRoleResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Role", ErrorMessage = "Role is not found" } });
            }
            return existingRole;
        }

        public async Task<Result<List<GetRoleUserResponse>>> GetRoleUsersAsync()
        {
            return await _iamDbConext.UserRoles.Where(ur => ur.User.IsServicePrincipal == false).ProjectTo<GetRoleUserResponse>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<Result<ErrorMessageVM>> UpdateStatusByIdAsync(RoleStatusRequest request)
        {
            var roles = await _iamDbConext.Roles.Where(r => r.Id == request.Id).FirstOrDefaultAsync();

            if (roles == null)
            {
                return Result<ErrorMessageVM>.Invalid(new List<ValidationError> { new ValidationError { Key = "Role", ErrorMessage = "Role Doesn't Exist" } });
            }

            roles.ModifiedById = _user.Id;
            roles.StatusId = request.IsActive ? 1 : 2;

            _iamDbConext.Update(roles);
            await _iamDbConext.SaveChangesAsync();
            return Result<ErrorMessageVM>.Success(new ErrorMessageVM { Id = roles.Id });
        }
    }
}
