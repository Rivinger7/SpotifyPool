using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Playlists
{
    public class PlaylistMapping : Profile
    {
        public PlaylistMapping()
        {
            CreateMap<Playlist, PlaylistsResponseModel>()
                .ReverseMap();
        }
    }
}
