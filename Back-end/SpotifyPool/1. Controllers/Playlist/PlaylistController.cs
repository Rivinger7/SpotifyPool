using BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Playlist
{
    [Route("api/v1/playlists")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class PlaylistController(IFavoritesPlaylist favoritesPlaylistService) : ControllerBase
    {
        private readonly IFavoritesPlaylist _favoritesPlaylistService = favoritesPlaylistService;

        /// <summary>
        /// Lấy danh sách các track yêu thích
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("favorite-songs")]
        public async Task<IActionResult> GetFavoriteSongs()
        {
            var result = await _favoritesPlaylistService.GetPlaylistAsync();
            return Ok(result);
        }

        /// <summary>
        /// Thêm track vào danh sách yêu thích
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("add-to-favorite-list")]
        public async Task<IActionResult> AddToFavoriteListAsync([FromBody] string trackID)
        {
            await _favoritesPlaylistService.AddToPlaylistAsync(trackID);
            return Ok(new { Message = "Add to Favorite Song Successfully" });
        }

        /// <summary>
        /// Xóa track ra khỏi danh sách yêu thích
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpDelete("playlist/favorite-songs/{trackID}")]
        public async Task<IActionResult> RemoveTrackFromFavoriteSongs(string trackID)
        {
            await _favoritesPlaylistService.RemoveFromPlaylistAsync(trackID);
            return Ok(new { Message = "Remove Favorite Song Successfully" });
        }
    }
}
