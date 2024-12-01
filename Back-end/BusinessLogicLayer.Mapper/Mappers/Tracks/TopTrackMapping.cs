using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
	public class TopTrackMapping : Profile
	{
		public TopTrackMapping()
		{
			// CreateMap<Track, TopTracksResponseModel>()
            //     // Chuyển ms sang giây (dùng Timespan)
            //     .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => (int)TimeSpan.FromMilliseconds(src.Duration).TotalSeconds))
            //     // Format lại Duration thành mm:ss
            //     .ForMember(dest => dest.DurationFormated, opt => opt.MapFrom(src => $"{src.Duration / (1000 * 60)}:{(src.Duration / 1000) % 60}"))
            //     .ReverseMap();

			CreateMap<ASTopTrack, Track>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Tracks.Select(x => x.Id).FirstOrDefault()))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Tracks.Select(x => x.Name).FirstOrDefault()))
				.ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Tracks.Select(x => x.Images).FirstOrDefault()))
				.ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Tracks.Select(x=>x.Duration)))
				//.ForMember(dest => dest.DurationFormated, opt => opt.MapFrom(src => $"{src.Duration / (1000 * 60)}:{(src.Duration / 1000) % 60}"))
				.ReverseMap();
		}
	}
}
