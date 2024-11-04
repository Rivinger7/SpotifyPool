using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;
namespace BusinessLogicLayer.Mapper.Mappers.Tracks
{
    public class AudioFeaturesFetchMapping : Profile
    {
        public AudioFeaturesFetchMapping()
        {
            CreateMap<SpotifyAudioFeaturesResponseModel, AudioFeatures>()
                .ReverseMap();
        }
    }
}
