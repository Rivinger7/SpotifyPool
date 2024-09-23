using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
    public class TrackFetchMapping : Profile
    {
        public TrackFetchMapping()
        {
            CreateMap<TrackResponseModel, Track>()
            .ForMember(dest => dest.SpotifyId, opt => opt.MapFrom(src => src.TrackId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ArtistIds, opt => opt.MapFrom(src => src.Artists.Select(a => a.Id).ToList()))
            .ForMember(dest => dest.Popularity, opt => opt.MapFrom(src => src.Popularity ?? 0))
            .ForMember(dest => dest.PreviewURL, opt => opt.MapFrom(src => src.PreviewURL))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration ?? 0))
            .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.ReleaseDate))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
            .ForMember(dest => dest.MarketsIds, opt => opt.MapFrom(src => src.AvailableMarkets.Select(m => m.Id).ToList()));

        }
    }
}
