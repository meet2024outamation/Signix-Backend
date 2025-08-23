using AutoMapper;
using Signix.IAM.API.Models;

namespace Signix.IAM.API.Endpoints.Role
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.dbo.Permission, PermissionDTO>().ReverseMap();
            CreateMap<Entities.dbo.RolePermission, RolePermissionDTO>()
              .ForMember(d => d.Role, o => o.MapFrom(s => s.Role))
              .ForMember(d => d.Permission, o => o.MapFrom(s => s.Permission));
            CreateMap<Entities.dbo.Role, GetRoleResponse>()
              .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client.Name))
              .ForMember(d => d.RoleStatus, o => o.MapFrom(s => s.Status.Name))
              .ForMember(d => d.Permissions, o => o.MapFrom(s => s.RolePermissions.Select(p => p.Permission)))
              .ForMember(d => d.NumberOfUsers, o => o.MapFrom(s => s.UserRoles.Count));

            CreateMap<RoleCreateRequest, Entities.dbo.Role>().ReverseMap();
            CreateMap<RoleUpdateRequest, Entities.dbo.Role>().ReverseMap();
            CreateMap<Entities.dbo.UserRole, GetRoleUserResponse>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
                .ForMember(d => d.Id, o => o.MapFrom(s => s.RoleId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Role.Name)).
                ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email));
        }
    }
}
