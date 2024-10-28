using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SetupLayer.Enum.Services.Playlist;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Playlists.Favorites
{
    public class FavoritesPlaylistBLL(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IFavoritesPlaylist
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task AddToPlaylistAsync(string trackID)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = _httpContextAccessor.HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(playlist => playlist.UserID == userID).FirstOrDefaultAsync();

            if (playlist is null)
            {
                playlist = new()
                {
                    Name = PlaylistName.FavoriteSong,
                    Description = string.Empty,
                    UserID = userID,
                    CreatedTime = Util.GetUtcPlus7Time(),
                    TrackIds = [trackID]
                };

                await _unitOfWork.GetCollection<Playlist>().InsertOneAsync(playlist);
                return;
            }

            // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
            if (playlist.TrackIds.Contains(trackID))
            {
                throw new DataExistCustomException("The song has been added to your Favorite Song");
            }

            playlist.TrackIds.Add(trackID);
            // Cập nhật playlist với danh sách TrackIds mới
            UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update.Set(p => p.TrackIds, playlist.TrackIds);
            await _unitOfWork.GetCollection<Playlist>().UpdateOneAsync(p => p.Id == playlist.Id, updateDefinition);
        }
    }
}
