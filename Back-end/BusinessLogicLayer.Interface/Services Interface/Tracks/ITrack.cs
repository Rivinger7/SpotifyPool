using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using Microsoft.AspNetCore.Http;
using SetupLayer.Enum.Services.Track;

namespace BusinessLogicLayer.Interface.Services_Interface.Tracks
{
    public interface ITrack
    {
        Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit, TrackFilterModel filterModel);
        Task<TrackResponseModel> GetTrackAsync(string id);
        Task UploadTrackAsync(UploadTrackRequestModel request);
        Task<IEnumerable<TrackResponseModel>> GetTracksByMoodAsync(Mood mood);
        Task FetchTracksByCsvAsync(IFormFile csvFile, string accessToken);

		//Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm);
		//Task<IEnumerable<TrackResponseModel>> GetTracksWithArtistIsNull();
	}
}
