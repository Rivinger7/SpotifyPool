using AutoMapper;
using Business_Logic_Layer.Models;
using Data_Access_Layer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
