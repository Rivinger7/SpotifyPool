using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Albums
{
    public class AlbumMapping : Profile
    {
        public AlbumMapping()
        {
            CreateMap<AlbumRequestModel, Album>()
                .ReverseMap();
        }
    }
}
