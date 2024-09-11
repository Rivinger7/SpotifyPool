using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper
{
    public class CustomerMappings : Profile
    {
        public CustomerMappings()
        {
            CreateMap<User, LoginRequestModel>().ReverseMap();
            CreateMap<User, RegisterRequestModel>().ReverseMap();
        }
    }
}
