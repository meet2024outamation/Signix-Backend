using AutoMapper;

namespace Signix.IAM.API.Endpoints.Client
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //CreateMap<Entities.Client, ClientCreateRequest>().ForMember(d => d.EmailAddress,
            //    o => o.MapFrom(s => s.Email)).ForMember(d => d.Phone,
            //    o => o.MapFrom(s => s.PhoneNumber));
            CreateMap<ClientCreateRequest, Entities.Client>().ForMember(d => d.Email,
                o => o.MapFrom(s => s.EmailAddress)).ForMember(d => d.PhoneNumber,
                o => o.MapFrom(s => s.Phone)).ForMember(d => d.Name,
                o => o.MapFrom(s => s.ClientName)).ForMember(d => d.Address,
                o => o.MapFrom(s => s.ClientAddress));
            CreateMap<ClientEditRequest,Entities.Client>()
                .ForMember(d => d.Email,
                o => o.MapFrom(s => s.EmailAddress)).ForMember(d => d.PhoneNumber,
                o => o.MapFrom(s => s.Phone)).ForMember(d => d.Name,
                o => o.MapFrom(s => s.ClientName)).ForMember(d => d.Address,
                o => o.MapFrom(s => s.ClientAddress))
                .ReverseMap();
            CreateMap<Entities.Client, ClientGetResponse>().ForMember(d => d.EmailAddress,
                o => o.MapFrom(s => s.Email)).ForMember(d => d.Phone,
                o => o.MapFrom(s => s.PhoneNumber)).ForMember(d => d.CreatedDate,
                o => o.MapFrom(s => s.CreatedDateTime)).ForMember(d => d.ClientName,
                o => o.MapFrom(s => s.Name)).ForMember(d => d.ClientAddress,
                o => o.MapFrom(s => s.Address))
                .ReverseMap();

            //Projection mapping
            //CreateProjection<Entities.Client, ClientGetResponse>();
                //.ForMember(d => d.Status, o => o.MapFrom(s => s.Status.Name));
            CreateProjection<Entities.UserClient, GetAccessibleClientResponse>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Client.Name))
                .ForMember(d => d.ClientId, o => o.MapFrom(s => s.ClientId))
                .ForMember(d => d.IsCurrentClient, o => o.MapFrom(s => s.User.CurrentClientId == s.ClientId))
                .ForMember(d => d.ClientType, o => o.MapFrom(s => s.Client.ClientTypeId == null ? null : s.Client.ClientType.Name))
                .ForMember(d => d.ClientTypeId, o => o.MapFrom(s => s.Client.ClientTypeId));
        }
    }
}