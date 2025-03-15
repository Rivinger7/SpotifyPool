using BusinessLogicLayer.Interface.Services_Interface.TopTracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.TopTrack
{
    [Route("api/top-tracks")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class TopTrackController(ITopTrack topTrackService) : ControllerBase
    {
        private readonly ITopTrack _topTrackService = topTrackService;

        [AllowAnonymous, HttpPatch()]
        public async Task<IActionResult> UpsertTopTrackAsync([FromForm] TopTrackRequestModel topTrackRequestModel)
        {
            await _topTrackService.UpsertTopTrackAsync(topTrackRequestModel);
            return Ok(new { Message = "Update Top Track Successfully" });
        }

        [AllowAnonymous, HttpGet()]
        public async Task<IActionResult> GetTopTrackAsync()
        {
            var result = await _topTrackService.GetTopTrackAsync();
            return Ok(result);
        }

        //[Authorize(Roles = $"{nameof(UserRole.Customer)}"), HttpPost()]
        //public async Task<IActionResult> UpdateStreamCountAsync([FromBody] string trackId)
        //{
        //    await _topTrackService.UpdateStreamCountAsync(trackId);
        //    return Ok(new { Message = "Update Stream Count Successfully" });
        //}
    }
}
