using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
	public class TrackFetchMapping : Profile
    {
        public TrackFetchMapping()
        {
            CreateMap<SpotifyTrackResponseModel, Track>()
            .ForMember(dest => dest.SpotifyId, opt => opt.MapFrom(src => src.SpotifyTrackId));
            //.ForMember(dest => dest.MarketsIds, opt => opt.MapFrom(src => src.AvailableMarkets.Select(m => m.Id).ToList()));
        }
    }
}
