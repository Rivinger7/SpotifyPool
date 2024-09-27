using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Artists
{
    public class ArtistFetchMapping : Profile
    {
        public ArtistFetchMapping()
        {
            CreateMap<SpotifyArtistResponseModel, Artist>()
                //.ForMember(dest => dest.SpotifyId, opt => opt.Ignore())
                //.ForMember(dest => dest.SpotifyId, opt => opt.MapFrom(src => src.SpotifyId))
                .ForMember(dest => dest.GenreIds, opt => opt.MapFrom(src => src.Genres))
                .ReverseMap();
        }
    }
}
