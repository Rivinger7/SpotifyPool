using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom
{
    public interface IPlaylist
    {
        Task<IEnumerable<PlaylistsResponseModel>> GetAllPlaylistsAsync();
        Task CreatePlaylistAsync(PlaylistRequestModel playlistRequestModel);
        Task AddToPlaylistAsync(string trackId, string playlistId);
        Task RemoveFromPlaylistAsync(string trackId, string playlistId);
        Task<PlaylistReponseModel> GetPlaylistAsync(string playlistId);
        Task<IEnumerable<TrackResponseModel>> GetRecommendationPlaylist(int offset, int limit);
        Task CreateMoodPlaylist(string mood);
    }
}
