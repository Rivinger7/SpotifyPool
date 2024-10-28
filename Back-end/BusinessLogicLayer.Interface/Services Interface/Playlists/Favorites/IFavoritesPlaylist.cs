namespace BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites
{
    public interface IFavoritesPlaylist
    {
        Task AddToPlaylistAsync(string trackID);
    }
}
