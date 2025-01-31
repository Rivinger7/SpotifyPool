using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using SetupLayer.Enum.Services.Track;

namespace BusinessLogicLayer.Interface.Services_Interface.Tracks
{
    public interface ITrack
    {
        Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit);
        Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm);
        Task<TrackResponseModel> GetTrackAsync(string id);
        Task UploadTrackAsync(UploadTrackRequestModel request);
        Task<IEnumerable<TrackResponseModel>> GetTracksByMoodAsync(Mood mood);
        //Task<IEnumerable<TrackResponseModel>> GetTracksWithArtistIsNull();
    }
}
