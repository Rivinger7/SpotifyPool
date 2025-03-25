using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Albums
{
    public class AlbumMapping : Profile
    {
        public AlbumMapping()
        {
            CreateMap<AlbumRequestModel, Album>()
                .ReverseMap();
            CreateMap<Album, AlbumResponseModel>();
                
                CreateMap<ReleaseMetadata, ReleaseMetadataResponse>()
                .ForMember(dest => dest.ReleasedTime, opt => opt.MapFrom(src => src.ReleasedTime.HasValue
                    ? src.ReleasedTime.Value.ToString("HH:mm:ss dd/MM/yyyy")
                    : null));
            CreateMap<Album, AlbumInfoResponse>();
        }
    }
}
