using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Account
{
	public class AccountMapping : Profile
	{
		public AccountMapping()
		{
			CreateMap<User, AccountResponse>()
				.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap();

			CreateMap<User, AccountDetailResponse>()
				.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime.ToString("HH:mm:ss dd/MM/yyyy")))
				.ForMember(dest => dest.LastLoginTime, opt => opt.MapFrom(src => src.LastLoginTime.HasValue
					? src.LastLoginTime.Value.ToString("HH:mm:ss dd/MM/yyyy")
					: null))
				.ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.LastUpdatedTime.HasValue
					? src.LastUpdatedTime.Value.ToString("HH:mm:ss dd/MM/yyyy")
					: null))
				.ReverseMap();

			CreateMap<UpdateRequestModel, User>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
				.ReverseMap();
		}
	}
}
