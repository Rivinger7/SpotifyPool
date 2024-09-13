using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper
{
    public class CustomerMappings : Profile
    {
        public CustomerMappings()
        {
            CreateMap<User, LoginRequestModel>().ReverseMap();
            CreateMap<User, RegisterRequestModel>().ReverseMap();
            CreateMap<User, UserResponseModel>()
                // Chuyển đổi ObjectId sang string
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ReverseMap();
        }
    }
}
