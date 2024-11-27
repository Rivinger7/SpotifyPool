using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom
{
    public interface IPlaylist
    {
        Task<IEnumerable<PlaylistsResponseModel>> GetAllPlaylistsAsync();
        Task CreatePlaylistAsync(string playlistName);
        Task AddToPlaylistAsync(string trackId, string playlistId);
        Task<IEnumerable<TrackPlaylistResponseModel>> GetTracksInPlaylistAsync(string playlistId);
        Task RemoveFromPlaylistAsync(string trackId, string playlistId);
    }
}
