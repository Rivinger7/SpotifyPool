using BusinessLogicLayer.ModelView.Service_Model_Views.AudioFeatures.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Interface.Services_Interface.Recommendation
{
    public interface IRecommendation
    {
        Task<IEnumerable<TrackResponseModel>> GetManyRecommendations(IEnumerable<string> trackIds, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 5);
        Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 5);
        Task<IEnumerable<TrackResponseModel>> GetRecommendations(AudioFeaturesRequest audioFeaturesRequest, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 1);
    }
}
