using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
    public class TrackFetchMapping : Profile
    {
        public TrackFetchMapping()
        {
            CreateMap<SpotifyTrackResponseModel, Track>()
            .ForMember(dest => dest.SpotifyId, opt => opt.MapFrom(src => src.TrackId))
            .ForMember(dest => dest.ArtistIds, opt => opt.MapFrom(src => src.Artists.Select(a => a.Id).ToList()))
            //.ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
            .ForMember(dest => dest.Images, opt => opt.Ignore());
            //.ForMember(dest => dest.MarketsIds, opt => opt.MapFrom(src => src.AvailableMarkets.Select(m => m.Id).ToList()));
        }
    }
}
