using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
    public class TrackMapping : Profile
    {
        public TrackMapping()
        {
            CreateMap<Track, TrackResponseModel>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ReverseMap();

            CreateMap<ASTrack, TrackResponseModel>()
                .ForMember(dest => dest.Artists, opt => opt.MapFrom(src => src.Artists))
                .ReverseMap();
        }
    }
}
