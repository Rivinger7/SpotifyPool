using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Albums
{
    public class AlbumMapping : Profile
    {
        public AlbumMapping()
        {
            CreateMap<AlbumRequestModel, Album>()
                .ReverseMap();
            CreateMap<Album, AlbumResponseModel>()
                .ForMember(dest => dest.ReleaseInfo.ReleasedTime, opt => opt.MapFrom(src => src.ReleaseInfo.ReleasedTime.HasValue
                    ? src.ReleaseInfo.ReleasedTime.Value.ToString("HH:mm:ss dd/MM/yyyy")
                    : null));
        }
    }
}
