using AutoMapper;
namespace Signix.IAM.API.Endpoints.Permission
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.dbo.Permission, GetPermissionResponse>().ReverseMap();
        }
    }
}
