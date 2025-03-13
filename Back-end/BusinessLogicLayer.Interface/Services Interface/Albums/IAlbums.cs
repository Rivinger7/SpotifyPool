using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using SetupLayer.Enum.Services.Album;

namespace BusinessLogicLayer.Interface.Services_Interface.Albums
{
    public interface IAlbums
    {
        Task<AlbumDetailResponseModel> GetAbumDetailByIdAsync(string albumId, bool? isSortByTrackName);
        Task<IEnumerable<AlbumResponseModel>> GetAlbumsAsync(PagingRequestModel paging, AlbumFilterModel model);
        Task CreateAlbumAsync(AlbumRequestModel request);
        Task UpdateAlbumAsync(string albumId, AlbumRequestModel request);
        Task DeleteAlbumAsync(string albumId);
        Task AddTracksToAlbum(IEnumerable<string> trackIds, string albumId);
        Task RemoveTracksFromAlbum(IEnumerable<string> trackIds, string albumId);
        Task ReleaseAlbumAsync(string albumId, DateTime releaseTime);
        Task ChangeAlbumStatusAsync(string albumId, ReleaseStatus status);
    }
}
