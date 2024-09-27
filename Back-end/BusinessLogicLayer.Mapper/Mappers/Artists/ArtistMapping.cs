using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Artists
{
    public class ArtistMapping : Profile
    {
        public ArtistMapping()
        {
            CreateMap<Artist, ArtistResponseModel>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
            .ReverseMap();
        }
    }
}
