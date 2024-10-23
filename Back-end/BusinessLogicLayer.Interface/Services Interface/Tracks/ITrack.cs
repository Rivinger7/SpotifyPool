using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Tracks
{
    public interface ITrack
    {
        Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync();
        Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm);
    }
}
