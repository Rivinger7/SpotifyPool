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
        private readonly IPlaylist _playlistService = playlistService;

		/// <summary>
		/// Lấy thông tin playlist bao gồm danh sách track
		/// </summary>
		/// <param name="id">Id của Playlist</param>
		/// <param name="filterModel">Sắp xếp. True: Asc, False: Desc</param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Customer)), HttpGet("{id}")]
        public async Task<IActionResult> GetPlaylistAsync(string id, [FromQuery] PlaylistFilterModel filterModel)
        {
            var result = await _playlistService.GetPlaylistAsync(id, filterModel);
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
            await _playlistService.CreatePlaylistAsync(playlistRequestModel);
            return Ok(new { Message = "Create Playlist Successfully" });
        }

        // Chưa xong
        ///// <summary>
        ///// Lấy danh sách track được đề xuất
        ///// </summary>
        ///// <param name="offset"></param>
        ///// <param name="limit"></param>
        ///// <returns></returns>
        //[AllowAnonymous, HttpGet("recommendation/tracks")]
        //public async Task<IActionResult> GetRecommendationPlaylist([FromQuery] int offset = 1, [FromQuery] int limit = 20)
        //{
        //    var result = await _playlistService.GetRecommendationPlaylist(offset, limit);
        //    return Ok(result);
        //}

        /// <summary>
        /// Thêm track vào playlist
        /// </summary>
        /// <param name="trackID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("{id}/tracks")]
        public async Task<IActionResult> AddToPlaylistAsync([FromBody] string trackID, string id)
        {
            await _playlistService.AddToPlaylistAsync(trackID, id);
            return Ok(new { Message = "Add to Playlist Successfully" });
        }

        /// <summary>
        /// Tạo playlist theo mood
        /// </summary>
        /// <param name="mood"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("mood")]
        public async Task<IActionResult> CreateMoodPlaylist([FromBody] string mood = "Sad")
        {
            await _playlistService.CreateMoodPlaylistAsync(mood);
            return Ok(new { Message = "Create Mood Playlist Successfully" });
        }

        /// <summary>
        /// Xóa track khỏi playlist
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Customer)), HttpDelete("{id}/tracks/{trackId}")]
        public async Task<IActionResult> RemoveTrackFromCustomPlaylist(string trackId, string id)
        {
            await _playlistService.RemoveFromPlaylistAsync(trackId, id);
            return Ok(new { Message = "Remove Custom Playlist Successfully" });
        }
    }
}
