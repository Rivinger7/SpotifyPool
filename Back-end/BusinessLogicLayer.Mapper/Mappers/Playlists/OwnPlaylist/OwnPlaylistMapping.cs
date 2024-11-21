using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Playlists.OwnPlaylist
{
	public class OwnPlaylistMapping : Profile
	{
		public OwnPlaylistMapping()
		{
			CreateMap<UpdatePlaylistRequestModel, Playlist>().ReverseMap().ForMember(dest => dest.Image, otp => otp.Ignore());
		}
	}
}
