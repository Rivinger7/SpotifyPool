using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Playlists.FavoriteSongs
{
    public class FavoriteSongsMapping : Profile
    {
        public FavoriteSongsMapping()
        {
            CreateMap<Playlist, FavoritesSongsResponseModel>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ReverseMap();
        }
    }
}
