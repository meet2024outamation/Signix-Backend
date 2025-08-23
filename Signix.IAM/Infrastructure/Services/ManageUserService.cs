using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using Signix.IAM.API.Models;
using Signix.IAM.Context;
using Signix.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using SharedKernel.Result;
using SharedKernel.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using static IAM.API.Infrastructure.Utility.Utility;
using User = IAM.Entities.User;


namespace Signix.IAM.API.Infrastructure.Services
{
    public class ManageUserService : ServiceBase, IManageUserService
    {
        private readonly IGraphService _graphService;
        private readonly IMapper _mapper;
        private readonly IUserClientServices _userClientServices;
        private readonly SystemSetting _systemSetting;
        private readonly IEmailServices _emailServices;
        private readonly IMemCacheService _memCacheService;
        private readonly AzureADConfig _azureADConfig;


        public ManageUserService(IAMDbContext iamDbConext, IGraphService graphService, IMapper mapper, IUserClientServices userClientServices, IOptions<SystemSetting> systemSetting, IEmailServices emailServices, IMemCacheService memCacheService, IOptions<AzureADConfig> azureADConfig, IUser user) : base(iamDbConext, user)
        {
            _graphService = graphService;
            _mapper = mapper;
            _userClientServices = userClientServices;
            _systemSetting = systemSetting.Value;
            _emailServices = emailServices;
            _memCacheService = memCacheService;
            _azureADConfig = azureADConfig.Value;
        }

        public async Task<Result<User>> CreateUser(UserCM userCM, bool isClientAdmin = false)
        {

            // We Can Check the Username as well as I(Paras) am thinking that the user name will be unique for everyone
            var isEmailExists = await IsEmailExists(userCM.Email, _user.Id);
            if (!isEmailExists.IsSuccess)
            {
                return Result<User>.Invalid(isEmailExists.ValidationErrors);
            }
            using var transaction = await _iamDbConext.Database.BeginTransactionAsync();
            userCM.ClientIds = userCM.ClientIds == null ? _user.CurrentClientId : userCM.ClientIds;
            userCM.CurrentClientId = userCM.ClientIds;
            var user = _mapper.Map<User>(userCM);
            user.CurrentClient = null;
            var azureAdUser = await _graphService.GetUserByEmail(user.Email);
            if (azureAdUser == null)
            {

                var invitation = await _graphService.InviteGuestUser(user);

                if (invitation == null || invitation.InvitedUser == null || invitation.InvitedUser.Id == null)
                    return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"{userCM.Email} is not added in Azure AD." } });

                user.AzureAduserId = invitation.InvitedUser.Id;
            }
            else
            {
                user.AzureAduserId = azureAdUser.Id;
            }
            if (!user.AzureAduserId.IsNullOrWhiteSpace())
                await _graphService.UserAssignment(user.AzureAduserId);
            user.Data = userCM.Data == null ? null : JsonSerializer.Serialize(userCM.Data);
            await _iamDbConext.Users.AddAsync(user);
            await _graphService.UpdateUser(user);
            await _iamDbConext.SaveChangesAsync();
            // will this work as it is adding the user in the user client table
            await _userClientServices.UpdateUserClientAsync(userCM.ClientIds, user.Id);
            //Need to pass login user id.
            if (isClientAdmin)
            {
                var role = await _iamDbConext.Roles.SingleAsync(r => r.Name == "All Permissions" && r.ClientId == _user.CurrentClientId);
                userCM.RoleIds.Add(role.Id);
            }
            
            if (userCM.RoleIds?.Count > 0)
            {

                var userRoles = userCM.RoleIds.Select(roleId => new UserRole { RoleId = roleId, UserId = user.Id }).ToList();
                await _iamDbConext.UserRoles.AddRangeAsync(userRoles);
                var userModules = await _iamDbConext.RolePermissions.Where(r => userCM.RoleIds.Contains(r.RoleId))
                    .Select(p => p.Permission.ModuleId)
                    .Distinct()
                    .Select(p => new UserClientModule
                    {
                        ModuleId = p,
                        UserId = user.Id,
                        ClientId = user.CurrentClientId,
                        ModifiedById = _user.Id
                    })
                                .ToListAsync();
                await _iamDbConext.UserClientModules.AddRangeAsync(userModules);
            }
            if (userCM.DepartmentIds?.Count > 0)
            {
                var userDepartments = userCM.DepartmentIds.Select(DepartmentId => new UserDepartment { DepartmentId = DepartmentId, UserId = user.Id }).ToList();
                await _iamDbConext.UserDepartments.AddRangeAsync(userDepartments);
            }
            await _iamDbConext.SaveChangesAsync();

            //User invitation email 
            var userInvite = _memCacheService.InviteUser;
            if (userInvite == null)
            {
                return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"Email template is not found." } });
            }
            var userObj = new
            {
                DisplayName = $"{userCM.FirstName} {userCM.LastName}",
                InviteLink = _azureADConfig.InviteRedirectUrl,
                AppName = _azureADConfig.ClientAppName
            };
            userInvite.To.Add(userCM.Email);
            await _emailServices.SendEmailWithTemplateAsync(userInvite, userObj);
            await transaction.CommitAsync();
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> DisableUser(string uniqueId, bool accountEnabled)
        {

            var existingUser = await _iamDbConext.Users.SingleOrDefaultAsync(u => u.UniqueId == uniqueId);
            if (existingUser == null || existingUser.AzureAduserId.IsNullOrWhiteSpace())
                return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"User is not found." } });
            // Code to disable the user from the azure ad as well
            await _graphService.DisabledUser(existingUser.AzureAduserId, accountEnabled);
            existingUser.IsActive = accountEnabled;
            await _iamDbConext.SaveChangesAsync();
            return Result<User>.Success(existingUser);
        }

        public async Task<Result<User>> DeleteUser(string uniqueId)
        {

            var existingUser = await _iamDbConext.Users.SingleOrDefaultAsync(u => u.UniqueId == uniqueId);
            if (existingUser == null || existingUser.AzureAduserId.IsNullOrWhiteSpace())
                return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"User is not found." } });
            // Code to disable the user from the azure ad as well
            await _graphService.DeleteUserByAzureADId(existingUser.AzureAduserId);
            existingUser.IsActive = false;
            existingUser.IsDeleted = true;
            _iamDbConext.Users.Update(existingUser);
            await _iamDbConext.SaveChangesAsync();
            return Result<User>.Success(existingUser);
        }

        public async Task<Result<UserEM>> GetUserById(int id)
        {
            var existingUser = await _iamDbConext.Users.Include(t => t.UserClients)
                    .SingleOrDefaultAsync(u => u.Id == id);

            if (existingUser == null)
                return Result<UserEM>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"User is not found." } });

            return Result<UserEM>.Success(_mapper.Map<UserEM>(existingUser));

        }

        public async Task<Result<UserVM>> GetUserByUniqueId(string uniqueId)
        {
            var existingUser = await _iamDbConext.Users
            .Where(u => u.IsServicePrincipal != true && u.UniqueId == uniqueId)
            .Select(u => new UserVM
            {
                
                CurrentClientId = _user.CurrentClientId,
                CurrentClientName = u.UserClients.Where(uc => uc.ClientId == _user.CurrentClientId).FirstOrDefault().Client.Name,
                UniqueId = u.UniqueId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Id = u.Id,
                IsActive = u.IsActive,
                IsServicePrincipal = u.IsServicePrincipal.Value,
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Modules = u.UserClientModules
                    .Where(ucm => ucm.Module != null && ucm.ClientId == _user.CurrentClientId)
                    .Select(uc => new ModuleDTO
                    {
                        Id = uc.ModuleId,
                        Name = uc.Module.Name,
                        Code = uc.Module.Code
                    }).ToList(),

                UserRoles = u.UserRoles
                    .Where(ur => ur.Role != null && ur.Role.ClientId == _user.CurrentClientId)
                    .Select(uc => new UserRoleDTO
                    {
                        RoleId = uc.RoleId,
                        RoleName = uc.Role.Name,
                        UserId = uc.UserId,
                        Permissions = uc.Role.RolePermissions.Select(p => new PermissionDTO
                        {
                            Id = p.PermissionId,
                            Name = p.Permission.Name,
                            Code = p.Permission.Code,
                            ModuleId = p.Permission.ModuleId,
                            ModuleName = p.Permission.Module.Name,
                            IsServicePrincipal = p.Permission.IsServicePrincipal.Value,
                            Type = null
                        }).ToList()
                    })
                    .ToList(),
                RoleIds = u.UserRoles
                    .Where(ur => ur.Role != null && ur.Role.ClientId == _user.CurrentClientId)
                    .Select(ur => ur.RoleId)
                    .ToList(),
                DepartmentIds = u.UserDepartments
                    .Where(ud => ud.Department != null && ud.Department.ClientId == _user.CurrentClientId)
                    .Select(ud => ud.Department.Id)
                    .ToList(),

                
            })
            .SingleOrDefaultAsync();


            if (existingUser == null)
                return Result<UserVM>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"User is not found." } });

            return Result<UserVM>.Success(existingUser);

        }


        public async Task<IEnumerable<UserList>> GetUsers(bool? isActive, string? currentClientId)
        {
            currentClientId = currentClientId == null ? _user.CurrentClientId : currentClientId;
            var client = await _iamDbConext.Clients.FindAsync(currentClientId);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Department, DepartmentObj>().ForMember(d => d.Id, o => o.MapFrom(s => s.Id)).
                ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Name));
                cfg.CreateMap<User, UserList>()
                   .ForMember(d => d.CurrentClientName, o => o.MapFrom(u => u.CurrentClient != null && u.IsActive ? u.CurrentClient.Name : string.Empty))
                    .ForMember(d => d.DepartmentIds, o => o.MapFrom(s => s.UserDepartments.Select(d => d.Department.Id)))
                   .ForMember(d => d.DepartmentNames, o => o.MapFrom(s => s.UserDepartments.Select(d => d.Department)))
                    .ForMember(d => d.RoleIds, o => o.MapFrom(s => s.UserRoles.Select(r => r.RoleId)))
                    .ForMember(d => d.RoleNames, o => o.MapFrom(s => s.UserRoles.Select(r => r.Role.Name)))
                .ForMember(d => d.AccessibleClientNames, o => o.MapFrom(u =>
                 u.UserClients.Where(t => t.Client.Status.Name == ClientStatusTypes.Active && t.UserId == _user.Id).Select(s => s.Client.Name).ToArray()
                ));
            });

            var mapper = config.CreateMapper();
            if (client?.Code.ToLower() == _systemSetting.BaseClientCode.ToLower()) 
            {
                if (isActive.HasValue && isActive.Value)
                {
                    return await _iamDbConext.Users.Where(u => u.UserClients.Any(uc => uc.ClientId == currentClientId) && u.IsActive == isActive.Value && u.IsServicePrincipal != true && !u.IsDeleted).ProjectTo<UserList>(mapper.ConfigurationProvider).ToListAsync();
                }
                return await _iamDbConext.Users.ProjectTo<UserList>(mapper.ConfigurationProvider).Where(x => !x.IsServicePrincipal).ToListAsync();

            }
            else
            {
                if (isActive.HasValue)
                {
                    return await _iamDbConext.Users.Where(u => u.UserClients.Any(uc => uc.ClientId == currentClientId) && u.IsActive == isActive.Value && u.IsServicePrincipal != true && !u.IsDeleted).OrderByDescending(u => u.CreatedDateTime).ProjectTo<UserList>(mapper.ConfigurationProvider).ToListAsync();
                }
                return await _iamDbConext.Users.Where(u => u.UserClients.Any(uc => uc.ClientId == currentClientId) && !u.IsDeleted).Where(x => x.IsServicePrincipal != true).OrderByDescending(u => u.CreatedDateTime).ProjectTo<UserList>(mapper.ConfigurationProvider).ToListAsync();
            }
        }

        public async Task<Result<User>> UpdateUser(UserEM userEM)
        {
            var existingUser = await _iamDbConext.Users
                .Include(s => s.UserClientModules)
                .Include(s => s.UserRoles)
                .Include(s => s.UserDepartments)
                .Include(s => s.UserClients)
                .SingleOrDefaultAsync(u => u.UniqueId == userEM.UniqueId && u.UserClients.Any(uc => uc.ClientId == _user.CurrentClientId) );

            if (existingUser == null)
                return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"User is not found." } });

            if (!existingUser.IsActive)
                return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"You can not change inactive user data." } });

            if (existingUser.Email != userEM.Email)
                return Result<User>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"You can not change email." } });
            userEM.CurrentClientId = userEM.ClientIds = existingUser.CurrentClientId == null? _user.CurrentClientId:existingUser.CurrentClientId;
            existingUser = _mapper.Map(userEM, existingUser);
            await _graphService.UpdateUser(existingUser);
            existingUser.Data = userEM.Data == null ? null : JsonSerializer.Serialize(userEM.Data);
            await _userClientServices.UpdateUserClientAsync(userEM.ClientIds, existingUser.Id);
            // We will remove user's every data from roles,modules and department
            _iamDbConext.UserRoles.RemoveRange(existingUser.UserRoles);
            _iamDbConext.UserClientModules.RemoveRange(existingUser.UserClientModules);
            _iamDbConext.UserDepartments.RemoveRange(existingUser.UserDepartments);

            if (userEM.RoleIds?.Count > 0)
            {
                var userRoles = userEM.RoleIds.Select(roleId => new UserRole { RoleId = roleId, UserId = existingUser.Id }).ToList();
                await _iamDbConext.UserRoles.AddRangeAsync(userRoles);
                var userModules = await _iamDbConext.RolePermissions.Where(r => userEM.RoleIds.Contains(r.RoleId))
                    .Select(p => p.Permission.ModuleId)
                    .Distinct()
                    .Select(p => new UserClientModule
                    {
                        ModuleId = p,
                        UserId = existingUser.Id,
                        ClientId = existingUser.CurrentClientId,
                        ModifiedById = _user.Id
                    }).ToListAsync();
                await _iamDbConext.UserClientModules.AddRangeAsync(userModules);
            }
            if (userEM.DepartmentIds?.Count > 0)
            {
                var userDepartments = userEM.DepartmentIds.Select(DepartmentId => new UserDepartment { DepartmentId = DepartmentId, UserId = existingUser.Id }).ToList();
                await _iamDbConext.UserDepartments.AddRangeAsync(userDepartments);
            }
            await _iamDbConext.SaveChangesAsync();
            return Result<User>.Success(existingUser);
        }

        public async Task<Result<UserEM>> GetUserByEmail(string email)
        {
            // shall we add is the user is active or not
            var existingUser = await _iamDbConext.Users.SingleOrDefaultAsync(s => s.Email == email);

            if (existingUser == null)
                return Result<UserEM>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"User is not found." } });

            return Result<UserEM>.Success(_mapper.Map<UserEM>(existingUser));
        }

        public async Task<Result<List<UserBasicInfo>>> GetActiveUsersByRolesAsync(string role)
        {
            var result = await _iamDbConext.Users.Where(u => u.IsActive && u.UserRoles.Any(r => r.Role.Name == role)).ProjectTo<UserBasicInfo>(_mapper.ConfigurationProvider).ToListAsync(); ;
            return Result<List<UserBasicInfo>>.Success(result);
        }

        public async Task<Result<List<UserBasicInfo>>> GetActiveUsersByPermissionAsync(string permission, string currentClientId)
        {
            var result = await _iamDbConext.Users.Where(u => u.IsActive && u.CurrentClientId == currentClientId && u.UserRoles.Any(r => r.Role.RolePermissions.Any(p => p.Permission.Code == permission))) // Here we are showing ServicePrincipal Users also Now
              .Include(u => u.UserDepartments)
              .ProjectTo<UserBasicInfo>(_mapper.ConfigurationProvider).Distinct().ToListAsync();
            return Result<List<UserBasicInfo>>.Success(result);
        }

        public async Task<Result<bool>> IsEmailExists(string email, int userId)
        {
            var existingUser = await _iamDbConext.Users.Include(c => c.UserClients)
                .SingleOrDefaultAsync(u => !u.IsDeleted && u.Email.ToLower() == email.ToLower());
            var currentUser = await _iamDbConext.Users.Include(c => c.CurrentClient) // client 
                .SingleOrDefaultAsync(u => !u.IsDeleted && u.Id == userId);
            // Here we are checking that in client we are adding this client does it already exist there
            if (existingUser != null && currentUser != null && currentUser.CurrentClient?.Code.ToLower() == _systemSetting.BaseClientCode.ToLower() || existingUser != null && currentUser != null && existingUser.UserClients.Any(u => u.ClientId == currentUser.CurrentClientId))
            {
                return Result<bool>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"{email} already has an account." } });
            }
            // this means that we are adding the in one more client 
            if (existingUser != null)
                return Result<bool>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = $"Kindly reach out to Administrator to add this user." } });

            return Result<bool>.Success(false);
        }

        public async Task<Result<UserDetail?>> GetUserDetailById(string clientId, string id)
        {
            var userDetail = await _iamDbConext.Users
            .Where(u => u.Email.ToLower() == id.ToLower())
            .Select(u => new UserDetail
            {
                CurrentClientId = clientId,
                CurrentClientName = u.UserClients.Where(uc => uc.ClientId == clientId).FirstOrDefault().Client.Name,
                UniqueId = u.UniqueId,  
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Id = u.Id,
                IsActive = u.IsActive,
                IsServicePrincipal = u.IsServicePrincipal.Value,
                UserName = u.UserName,
                MiddleName = u.MiddleName,
                AccessibleClient = u.UserClients
                .Select(uc => new AccessibleClientDTO
                {
                    ClientId = uc.ClientId,
                    Name = uc.Client.Name,
                    IsCurrentClient = u.CurrentClientId == uc.ClientId,
                    ClientType = uc.Client.ClientType != null ? uc.Client.ClientType.Name : null,
                    ClientTypeId = uc.Client.ClientTypeId
                }).ToList(),

                Modules = u.UserClientModules
                    .Where(ucm => ucm.Module != null && ucm.ClientId == clientId)
                    .Select(uc => new ModuleDTO
                    {
                        Id = uc.ModuleId,
                        Name = uc.Module.Name,
                        Code = uc.Module.Code
                    }).ToList(),
                    
                UserRoles = u.UserRoles
                    .Where(ur => ur.Role != null && ur.Role.ClientId == clientId)
                    .Select(uc => new UserRoleDTO
                    {
                        RoleId = uc.RoleId,
                        RoleName = uc.Role.Name,
                        UserId = uc.UserId,
                        Permissions = uc.Role.RolePermissions.Select(p => new PermissionDTO
                        {
                            Id = p.PermissionId,
                            Name = p.Permission.Name,
                            Code = p.Permission.Code,
                            ModuleId = p.Permission.ModuleId,
                            ModuleName = p.Permission.Module.Name,
                            IsServicePrincipal = p.Permission.IsServicePrincipal.Value,
                            Type = null
                        }).ToList()
                    })
                    .ToList(),

                Departments = u.UserDepartments
                    .Where(ud => ud.Department != null && ud.Department.ClientId == clientId)
                    .Select(ud => new DepartmentDTO
                    {
                        Id = ud.DepartmentId,
                        Name = ud.Department.Name,
                       
                    })
                    .ToList(),

                DepartmentIds = u.UserDepartments
                    .Where(ud => ud.Department != null && ud.Department.ClientId == clientId)
                    .Select(ud => ud.Department.Id)
                    .ToHashSet(),

                DepartmentNames = u.UserDepartments
                    .Where(ud => ud.Department != null && ud.Department.ClientId == clientId)
                    .Select(ud => new DepartmentObj
                    {
                        Id = ud.DepartmentId,
                        DepartmentName = ud.Department.Name,

                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
            return userDetail;
        }

        public async Task<Result<List<AccessibleClientDTO>?>> GetUserAccessibleClientsById(string id)
        {
            var userDetail = await _iamDbConext.Users
                .ProjectTo<UserDetail>(_mapper.ConfigurationProvider).SingleOrDefaultAsync(u => u.Email.ToLower() == id.ToLower());
            return userDetail?.AccessibleClient.ToList();
        }

        public async Task<IList<string>> GetEmails()
        {
            return await _iamDbConext.Users.Select(s => s.Email).ToListAsync();
        }


        public async Task<Result<int>> LoginUser(string accessToken)
        {
            Jwttoken jwttoken = new Jwttoken
            {
                UserId = _user.Id,
                Token = Utility.Utility.ParsedToken(accessToken).Signature,
                ExpirationDate = Utility.Utility.ParsedToken(accessToken).TokenExpiryUtcDate
            };

            await _iamDbConext.Jwttokens.AddAsync(jwttoken);
            int affectedRows = await _iamDbConext.SaveChangesAsync();

            //Delete expired tokens
            await _iamDbConext.Jwttokens.Where(j => j.ExpirationDate < DateTime.UtcNow).ExecuteDeleteAsync();

            return Result<int>.Success(affectedRows);
        }
        public async Task<Result<int>> LogoutUser(string accessToken)
        {
            await _memCacheService.RemoveCache(_user.Email!);
            string signature = Utility.Utility.ParsedToken(accessToken).Signature;
            return await _iamDbConext.Jwttokens.Where(x => x.UserId == _user.Id && x.Token == signature).ExecuteDeleteAsync();
        }
    }
}
