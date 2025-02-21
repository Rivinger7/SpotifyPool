using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Track
{
	[Route("api/v1/tracks")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class TrackController(ITrack trackService) : ControllerBase
    {
        private readonly ITrack _trackService = trackService;

        // Chưa xong
        //[AllowAnonymous, HttpPost("fetch/csv")]
        //public async Task<IActionResult> FetchTracksByCsvAsync(IFormFile csvFile, string accessToken)
        //{
        //    await _trackService.FetchTracksByCsvAsync(csvFile, accessToken);
        //    return Ok("Fetch successfully!");
        //}

        //[AllowAnonymous, HttpGet("testing/tracks-with-artist-null")]
        //public async Task<IActionResult> GetTracksArtistNull()
        //{
        //    var result = await _trackService.GetTracksWithArtistIsNull();
        //    return Ok(result);
        //}

        /// <summary>
        /// Lấy track theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetTrackByIdAsync([FromRoute] string id)
        {
            var result = await _trackService.GetTrackAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả track
        /// </summary>
        /// <param name="filterModel">Lọc</param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet()]
        public async Task<IActionResult> GetAllTracksAsync([FromQuery] TrackFilterModel filterModel, [FromQuery]int offset = 1, [FromQuery]int limit = 10)
        {
            var result = await _trackService.GetAllTracksAsync(offset, limit, filterModel);
            return Ok(result);
        }

        #region SearchTracksAsync (Code này được gộp vào trong GetAllTrack)
        ///// <summary>
        ///// Tìm theo tên Track hoặc Artist
        ///// </summary>
        ///// <param name="searchTerm"></param>
        ///// <returns></returns>
        //[AllowAnonymous, HttpGet("search")]
        //public async Task<IActionResult> SearchTracksAsync([FromQuery] string searchTerm)
        //{
        //    var result = await _trackService.SearchTracksAsync(searchTerm);
        //    return Ok(result);
        //}
		#endregion


		//[AllowAnonymous, HttpGet("filter")]
		//public async Task<IActionResult> GetTracksByMoodAsync([FromQuery] Mood mood)
		//{
		//    var result = await _trackService.GetTracksByMoodAsync(mood);
		//    return Ok(result);
		//}

		[Authorize(Roles = $"{nameof(UserRole.Artist)}"), HttpPost("upload")]
        public async Task<IActionResult> UploadTrackAsync([FromForm] UploadTrackRequestModel request)
        {
            await _trackService.UploadTrackAsync(request);
            return Ok("Upload successfully!");
        }
	}
}
