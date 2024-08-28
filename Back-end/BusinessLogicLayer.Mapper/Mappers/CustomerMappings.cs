using AutoMapper;
using Business_Logic_Layer.Models;
using Data_Access_Layer.Entities;

namespace Business_Logic_Layer.Mappers
{
    public class CustomerMappings : Profile
    {
        public CustomerMappings()
        {
            CreateMap<User, CustomerModel>().ReverseMap();
            CreateMap<User, LoginModel>().ReverseMap();
            CreateMap<User, RegisterModel>().ReverseMap();
        }
    }
}
