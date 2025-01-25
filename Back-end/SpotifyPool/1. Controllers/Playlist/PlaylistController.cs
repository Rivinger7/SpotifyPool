using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
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
        /// Lấy thông tin playlist bao gồm danh sách track
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("{id}")]
        public async Task<IActionResult> GetPlaylistAsync(string id)
        {
            var result = await playlistService.GetPlaylistAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo playlist mới
        /// </summary>
        /// <param name="playlistRequestModel"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost]
        public async Task<IActionResult> CreatePlaylistAsync([FromForm] PlaylistRequestModel playlistRequestModel)
        {
            await playlistService.CreatePlaylistAsync(playlistRequestModel);
            return Ok(new { Message = "Create Playlist Successfully" });
        }

        /// <summary>
        /// Lấy danh sách track được đề xuất
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("recommendation/tracks")]
        public async Task<IActionResult> GetRecommendationPlaylist([FromQuery] int offset = 1, [FromQuery] int limit = 20)
        {
            var result = await playlistService.GetRecommendationPlaylist(offset, limit);
            return Ok(result);
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
        /// Tạo playlist theo mood
        /// </summary>
        /// <param name="mood"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("create-playlist-by-mood")]
        public async Task<IActionResult> CreateMoodPlaylist([FromQuery] string mood = "Sad")
        {
            await playlistService.CreateMoodPlaylist(mood);
            return Ok(new { Message = "Create Mood Playlist Successfully" });
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
