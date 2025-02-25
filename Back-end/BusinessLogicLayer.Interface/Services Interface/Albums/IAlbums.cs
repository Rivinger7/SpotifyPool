using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;

namespace BusinessLogicLayer.Interface.Services_Interface.Albums
{
    public interface IAlbums
    {
        Task CreateAlbumAsync(AlbumRequestModel request);
        Task DeleteAlbumAsync(string albumId);
        Task UpdateAlbumAsync(string albumId, AlbumRequestModel request);
    }
}
