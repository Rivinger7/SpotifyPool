using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Playlist
{
    [Route("api/v1/playlists")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class PlaylistController(IPlaylist playlistService) : ControllerBase
    {
        private readonly IPlaylist playlistService = playlistService;

        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet]
        public async Task<IActionResult> GetAllPlaylistsAsync()
        {
            var result = await playlistService.GetAllPlaylistsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách track trong playlist
        /// </summary>
        /// <param name="id">Playlist Id</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("{id}")]
        public async Task<IActionResult> GetTrackPlaylist(string id)
        {
            var result = await playlistService.GetTracksInPlaylistAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo playlist mới
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("{playlistName}")]
        public async Task<IActionResult> CreatePlaylistAsync(string playlistName)
        {
            await playlistService.CreatePlaylistAsync(playlistName);
            return Ok(new { Message = "Create Playlist Successfully" });
        }

        /// <summary>
        /// Thêm track vào playlist
        /// </summary>
        /// <param name="trackID"></param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("{playlistId}/add-track")]
        public async Task<IActionResult> AddToPlaylistAsync([FromBody] string trackID, string playlistId)
        {
            await playlistService.AddToPlaylistAsync(trackID, playlistId);
            return Ok(new { Message = "Add to Playlist Successfully" });
        }

        /// <summary>
        /// Xóa track khỏi playlist
        /// </summary>
        /// <param name="trackID"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpDelete("{playlistID}")]
        public async Task<IActionResult> RemoveTrackFromCustomPlaylist(string trackID, string playlistID)
        {
            await playlistService.RemoveFromPlaylistAsync(trackID, playlistID);
            return Ok(new { Message = "Remove Custom Playlist Successfully" });
        }
    }
}
