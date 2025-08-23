using AutoMapper;

namespace Signix.IAM.API.Endpoints.UserPermission
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.dbo.UserClientModule, UpdateUserModuleRequest>().ReverseMap();
        }
    }
}
