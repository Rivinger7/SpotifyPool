using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Track
{
    [Route("api/v1/tracks")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class TrackController(ITrack trackService) : ControllerBase
    {
        private readonly ITrack _trackService = trackService;

        /// <summary>
        /// Lấy track theo ID
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("{trackId}")]
        public async Task<IActionResult> GetTrackByIdAsync([FromRoute] string trackId)
        {
            var result = await _trackService.GetTrackAsync(trackId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả các tracks
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpGet()]
        public async Task<IActionResult> GetAllTracksAsync([FromQuery]int offset = 1, [FromQuery]int limit = 10)
        {
            var result = await _trackService.GetAllTracksAsync(offset, limit);
            return Ok(result);
        }

        /// <summary>
        /// Tìm theo tên Track hoặc Artist
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("search")]
        public async Task<IActionResult> SearchTracksAsync([FromQuery] string searchTerm)
        {
            var result = await _trackService.SearchTracksAsync(searchTerm);
            return Ok(result);
        }

        [AllowAnonymous, HttpGet("top-track")]
		public async Task<IActionResult> GetTopTracksAsync()
		{
			var result = await _trackService.GetTopTracksAsync();
			return Ok(result);
		}
	}
}
