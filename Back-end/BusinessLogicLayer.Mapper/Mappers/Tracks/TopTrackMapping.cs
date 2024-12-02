using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Response;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
	public class TopTrackMapping : Profile
	{
		public TopTrackMapping()
		{
			CreateMap<ASTopTrack, TopTrackResponseModel>()
				.ReverseMap();

			CreateMap<TopTracksInfo, TracksInfoResponse>()
				.ReverseMap();

			CreateMap<Track, TrackInTopTrackResponseModel>()
				// Chuyển ms sang giây (dùng Timespan)
				.ForMember(dest => dest.Duration, opt => opt.MapFrom(src => (int)TimeSpan.FromMilliseconds(src.Duration).TotalSeconds))
				// Format lại Duration thành mm:ss
				.ForMember(dest => dest.DurationFormated, opt => opt.MapFrom(src => $"{src.Duration / (1000 * 60)}:{(src.Duration / 1000) % 60}"))
				.ReverseMap();
		}
	}
}
