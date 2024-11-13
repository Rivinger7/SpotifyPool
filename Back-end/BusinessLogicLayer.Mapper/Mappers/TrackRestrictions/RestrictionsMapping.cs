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
            .ReverseMap();
        }
    }
}
