using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Aggregate_Storage;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
    public class TopTrackMapping : Profile
	{
		public TopTrackMapping()
		{
			CreateMap<ASTopTrack, TrackResponseModel>()
				.ReverseMap();
		}
	}
}
