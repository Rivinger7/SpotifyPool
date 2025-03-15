using BusinessLogicLayer.ModelView.Service_Model_Views.AudioFeatures.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using SetupLayer.Enum.Services.Reccomendation;

namespace BusinessLogicLayer.Interface.Services_Interface.Recommendation
{
    public interface IRecommendation
    {
        Task<IEnumerable<TrackResponseModel>> GetManyRecommendations(IEnumerable<string> trackIds, Algorithm algorithm, int k = 5);
        Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, Algorithm algorithm, int k = 5);
        Task<IEnumerable<TrackResponseModel>> GetRecommendations(AudioFeaturesRequest audioFeaturesRequest, Algorithm algorithm, int k = 7);
    }
}
