using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Aggregate_Storage;

namespace BusinessLogicLayer.Interface.Services_Interface.Tracks
{
    public interface ITrack
    {
        Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit);
        Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm);
        Task<TrackResponseModel> GetTrackAsync(string id);
    }
}
