using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Artists
{
    public class ArtistFetchMapping : Profile
    {
        public ArtistFetchMapping()
        {
            CreateMap<SpotifyArtistResponseModel, Artist>()
                .ReverseMap();
        }
    }
}
