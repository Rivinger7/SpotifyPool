using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using DataAccessLayer.Repository.Entities;


namespace BusinessLogicLayer.Mapper.Mappers.Users
{
	public class UserMapping : Profile
	{
		public UserMapping()
		{
			// Map between Image classes
			CreateMap<EditProfileRequestModel, User>().ReverseMap();


			CreateMap<User, UserProfileResponseModel>()
				.ForMember(dest => dest.Id, otp => otp.MapFrom(src => src.Id))
				.ForMember(dest => dest.Avatar, otp => otp.MapFrom(src => src.Images))
				.ForMember(dest => dest.Name, otp => otp.MapFrom(src => src.DisplayName))
				.ReverseMap();
		}
	}
}
