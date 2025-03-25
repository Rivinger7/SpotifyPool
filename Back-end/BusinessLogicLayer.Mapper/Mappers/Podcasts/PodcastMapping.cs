using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Podcast.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Podcasts;

public class PodcastMapping : Profile
{
    public PodcastMapping()
    {
        CreateMap<Podcast, PodcastResponseModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher.ToString()))
            .ForMember(dest => dest.Host, opt => opt.MapFrom(src => src.Host.ToString()))
            .ForMember(dest => dest.Episodes, opt => opt.MapFrom(src => src.Episodes.Select(e => e.ToString())))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime.ToString("HH:mm:ss dd/MM/yyyy")))
            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.CreatedTime.ToString("HH:mm:ss dd/MM/yyyy")));
    }
}