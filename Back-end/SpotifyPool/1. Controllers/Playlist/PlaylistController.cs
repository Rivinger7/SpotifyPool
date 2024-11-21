using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
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
    public class PlaylistController(IFavoritesPlaylist favoritesPlaylistService, ICustomPlaylist customPlaylistService) : ControllerBase
    {
        private readonly IFavoritesPlaylist _favoritesPlaylistService = favoritesPlaylistService;
        private readonly ICustomPlaylist _customPlaylistService = customPlaylistService;

        #region Favorites Playlist
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
        #endregion

        #region Custom Playlist
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("custom-playlist/{id}")]
        public async Task<IActionResult> GetCustomPlaylist(string id)
        {
            var result = await _customPlaylistService.GetPlaylistAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("create-custom-playlist")]
        public async Task<IActionResult> CreateCustomPlaylistAsync([FromBody] string playlistName)
        {
            await _customPlaylistService.CreatePlaylistAsync(playlistName);
            return Ok(new { Message = "Create Custom Playlist Successfully" });
        }

        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("add-to-custom-playlist")]
        public async Task<IActionResult> AddToCustomPlaylistAsync([FromBody] string trackID, string playlistID)
        {
            await _customPlaylistService.AddToPlaylistAsync(trackID, playlistID);
            return Ok(new { Message = "Add to Custom Playlist Successfully" });
        }

        [Authorize(Roles = nameof(UserRole.Customer)), HttpDelete("playlist/custom-playlist/{playlistID}")]
        public async Task<IActionResult> RemoveTrackFromCustomPlaylist(string trackID, string playlistID)
        {
            await _customPlaylistService.RemoveFromPlaylistAsync(trackID, playlistID);
            return Ok(new { Message = "Remove Custom Playlist Successfully" });
        }

        #endregion
    }
}
