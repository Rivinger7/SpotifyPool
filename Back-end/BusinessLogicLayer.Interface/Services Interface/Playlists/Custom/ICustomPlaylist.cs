using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom
{
    public interface ICustomPlaylist
    {
        Task CreatePlaylistAsync(string playlistName);
        Task AddToPlaylistAsync(string trackId, string playlistId);
        Task<FavoritesSongsResponseModel> GetPlaylistAsync(string playlistId);
        Task RemoveFromPlaylistAsync(string trackId, string playlistId);
    }
}
