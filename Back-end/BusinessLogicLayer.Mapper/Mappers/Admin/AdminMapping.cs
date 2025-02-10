using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Admin
{
	public class AdminMapping : Profile
	{
		public AdminMapping()
		{
			CreateMap<User, AdminResponse>()
				.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap();

			CreateMap<User, AdminDetailResponse>()
				.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap();
		}
	}
}
