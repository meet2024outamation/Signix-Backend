using AutoMapper;
using Newtonsoft.Json;
using System.Text.Json;
using Signix.IAM.API.Models;
using static IAM.API.Infrastructure.Utility.Utility;
using SharedKernel.Services;
using Signix.IAM.Entities;
using SharedKernel.AuthorizeHandler;

namespace Signix.IAM.API.Infrastructure.Utility
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<User, UserEM>()
                .ForMember(d => d.Data, o => o.MapFrom(u => u.Data == null ? null : JsonConvert.DeserializeObject<BarNotary>(u.Data)))
                .ForMember(d => d.RoleIds, o => o.MapFrom(u => u.UserRoles.Select(r => r.RoleId)))
                .ForMember(d => d.CurrentClientName, o => o.MapFrom(u => u.CurrentClient != null && u.IsActive ? u.CurrentClient.Name : string.Empty))
                //.ForMember(d => d.DepartmentNames, o => o.MapFrom(u => u.UserDepartments.Select(s => s.Department.Name).ToArray()))
                .ForMember(d => d.DepartmentIds, o => o.MapFrom(u => u.UserDepartments.Select(s => s.DepartmentId)))
                .ForMember(d => d.ClientIds, o => o.MapFrom(u => u.UserClients.Select(s => s.ClientId)));

            CreateMap<User, UserBasicInfo>()
                .ForMember(d => d.DepartmentIds, o => o.MapFrom(u => u.UserDepartments.Select(d => d.DepartmentId)))
                .ForMember(d => d.DepartmentNames, o => o.MapFrom(u => u.UserDepartments.Select(d => d.Department.Name)))
                .ReverseMap();

            CreateMap<UserEM, User>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Data, o => o.Ignore());

            CreateMap<User, UserCM>()
                .ForMember(d => d.Data, o => o.MapFrom(u => u.Data == null ? null : JsonConvert.DeserializeObject<BarNotary>(u.Data)))
                .ReverseMap();
            CreateProjection<Department, DepartmentObj>().ForMember(d => d.Id, o => o.MapFrom(s => s.Id)).
                ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Name));
            CreateProjection<User, UserWithClients>()
               // .ForMember(s => s.CurrentClientCode, o => o.MapFrom(s => s.CurrentClient.Code))
                .ForMember(s => s.Permissions, o => o.MapFrom(s => s.UserRoles.SelectMany(ur =>
                        ur.Role.RolePermissions.Select(rp => rp.Permission.Code))))
                .ForMember(s => s.Modules, o => o.MapFrom(s => s.UserClientModules.Select(ur =>
                        ur.Module.Code)))
              //  .ForMember(d => d.ConnectionString, o => o.MapFrom(s => s.CurrentClient!.ConnectionString))
                .ForMember(d => d.DepartmentIds, o => o.MapFrom(s => s.UserDepartments.Select(ud => ud.DepartmentId)))
                .ForMember(d => d.DepartmentNames, o => o.MapFrom(s => s.UserDepartments.Select(ud => ud.Department)))
                .ForMember(dest => dest.CurrentClientName, opt => opt.MapFrom(src => src.CurrentClient.Name));
            CreateProjection<User, UserVM>()
                .ForMember(d => d.Modules, o => o.MapFrom(s => s.UserClientModules.Select(m => m.Module)))
                .ForMember(d => d.UserRoles, o => o.MapFrom(s => s.UserRoles.Select(x =>
                     new UserRoleDTO
                     {
                         RoleId = x.RoleId,
                         UserId = x.UserId,
                         RoleName = x.Role.Name,
                         Permissions = x.Role.RolePermissions.Select(rp =>
                             new PermissionDTO
                             {
                                 Id = rp.PermissionId,
                                 Name = rp.Permission.Name,
                                 Code = rp.Permission.Code,
                                 ModuleId = rp.Permission.ModuleId,
                                 ModuleName = rp.Permission.Module.Name,
                                 IsServicePrincipal = rp.Permission.IsServicePrincipal ?? false
                             }).ToList()
                     }
                )))
                .ForMember(d => d.Data, o => o.MapFrom(u => u.Data == null ? null : JsonConvert.DeserializeObject<BarNotary>(u.Data)))
                .ForMember(d => d.RoleIds, o => o.MapFrom(u => u.UserRoles.Select(r => r.RoleId)))
                .ForMember(d => d.CurrentClientName, o => o.MapFrom(u => u.CurrentClient != null && u.IsActive ? u.CurrentClient.Name : string.Empty))
                //.ForMember(d => d.DepartmentNames, o => o.MapFrom(u =>  u.UserDepartments.Select(s => s.Department.Name).ToArray()))
                .ForMember(d => d.DepartmentIds, o => o.MapFrom(u => u.UserDepartments.Select(s => s.DepartmentId)))
                .ForMember(d => d.ClientIds, o => o.MapFrom(u => u.UserClients.Select(s => s.ClientId)));
        }

    }
}
