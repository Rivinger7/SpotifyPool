using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Images
{
    public class ImageMapping : Profile
    {
        public ImageMapping()
        {
            // Map between Image classes
            CreateMap<ImageResponseModel, Image>().ReverseMap();
        }
    }
}
