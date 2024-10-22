using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using DataAccessLayer.Repository.Entities;


namespace BusinessLogicLayer.Mapper.Mappers.Users
{
	public class UserMapping : Profile
	{
		public UserMapping()
		{
			// Map between Image classes
			CreateMap<EditProfileRequestModel, User>().ReverseMap().ForMember(dest => dest.Image, otp => otp.Ignore());
		}
	}
}
