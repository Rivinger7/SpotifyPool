using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
    public class TrackUploadMapping : Profile
    {
        public TrackUploadMapping()
        {
            CreateMap<UploadTrackRequestModel, Track>()
                .ForMember(dest => dest.Restrictions, opt => opt.MapFrom(src => new Restrictions
                {
                    IsPlayable = src.Restrictions.IsPlayable,
                    Reason = src.Restrictions.Reason
                }))
                .ReverseMap();
        }
    }
}
