using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Episode.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.Episodes;

public class EpisodeMapping : Profile
{
    public EpisodeMapping()
    {
        CreateMap<Episode, EpisodeResponseModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.PodcastId, opt => opt.MapFrom(src => src.PodcastId.ToString()));
    }
}