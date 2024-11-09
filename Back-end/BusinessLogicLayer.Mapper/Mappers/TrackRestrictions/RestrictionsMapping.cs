using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Restrictions.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.TrackRestrictions
{
    public class RestrictionsMapping : Profile
    {
        public RestrictionsMapping()
        {
            CreateMap<RestrictionsResponseModel, Restrictions>()
            //.ForMember(dest => dest.IsPlayable, opt => opt.MapFrom(src => src.IsPlayable))
            //.ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason.ToString()));
            .ReverseMap();
        }
    }
}
