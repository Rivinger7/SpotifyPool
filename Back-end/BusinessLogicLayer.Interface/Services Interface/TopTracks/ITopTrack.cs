using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Interface.Services_Interface.TopTracks
{
    public interface ITopTrack
    {
        Task<IEnumerable<TrackResponseModel>> GetTopTrackAsync();
        Task UpsertTopTrackAsync(TopTrackRequestModel topTrackRequestModel);
        Task UpdateStreamCountAsync(string trackId);
    }
}
