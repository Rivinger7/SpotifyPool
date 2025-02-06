using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.AudioFeatures.Request;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Mapper.Mappers.AudioFeaturesMap
{
    public class AudioFeaturesMapping : Profile
    {
        public AudioFeaturesMapping()
        {
            CreateMap<AudioFeatures, AudioFeaturesRequest>()
                .ReverseMap();
        }
    }
}
