using AutoMapper;
using Signix.IAM.API.Models;

namespace Signix.IAM.API.Endpoints.User
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateProjection<Entities.dbo.UserClient, AccessibleClientDTO>()
               .ForMember(d => d.Name, o => o.MapFrom(s => s.Client.Name))
               .ForMember(d => d.ClientId, o => o.MapFrom(s => s.ClientId))
               .ForMember(d => d.IsCurrentClient, o => o.MapFrom(s => s.User.CurrentClientId == s.ClientId))
               .ForMember(d => d.ClientType, o => o.MapFrom(s => s.Client != null))
               .ForMember(d => d.ClientTypeId, o => o.MapFrom(s => s.Client.ClientTypeId));

            CreateProjection<Entities.dbo.Permission, PermissionDTO>()
               .ForMember(d => d.ModuleId, o => o.MapFrom(s => s.ModuleId))
               .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.Module.Name));

            CreateProjection<Entities.dbo.Module, ModuleDTO>();
            CreateProjection<Entities.dbo.UserRole, UserRoleDTO>()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => s.Role.RolePermissions.Select(s => s.Permission)))
                .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role.Name));

            CreateProjection<Entities.dbo.User, UserDetail>()
                   .ForMember(d => d.AccessibleClient, o => o.MapFrom(s => s.UserClients))
                   .ForMember(d => d.Modules, o => o.MapFrom(s => s.UserClientModules.Select(m => m.Module)))
                   .ForMember(d => d.UserRoles, o => o.MapFrom(s => s.UserRoles));
            //.ForMember(d => d.DepartmentNames, o => o.MapFrom(s => s.UserDepartments.Select(d => d.Department)))
            //.ForMember(d => d.Departments, o => o.MapFrom(s => s.UserDepartments.Select(d => d.Department)))
            //.ForMember(d => d.DepartmentIds, o => o.MapFrom(s => s.UserDepartments.Select(d => d.Department.Id)));

        }
    }
}
