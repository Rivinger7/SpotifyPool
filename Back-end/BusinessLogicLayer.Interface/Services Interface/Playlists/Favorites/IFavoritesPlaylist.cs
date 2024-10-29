using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites
{
    public interface IFavoritesPlaylist
    {
        Task AddToPlaylistAsync(string trackID);
        Task RemoveFromPlaylistAsync(string trackID);
        Task<FavoritesSongsResponseModel> GetPlaylistAsync();
    }
}
