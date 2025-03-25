using BusinessLogicLayer.Interface.Services_Interface.Artists;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Artist
{
    [Route("api/v1/artists")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class ArtistController(IArtist artistService) : ControllerBase
    {
        private readonly IArtist _artistService = artistService;

        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("register")]
        public async Task<IActionResult> CreateArtist([FromForm] ArtistRequest artistRequest)
        {
            await _artistService.CreateArtist(artistRequest);
            return Ok(new { Message = "Create Artist Successfully" });
        }

        [Authorize(Roles = nameof(UserRole.Artist)), HttpPost("me/profile-switch")]
        public async Task<IActionResult> SwitchToUserProfile()
        {
            var authenticatedResponseModel = await _artistService.SwitchToUserProfile();
            return Ok(new { Message = "Switch Profile Successfully", authenticatedResponseModel });
        }

        [Authorize(Roles = nameof(UserRole.Artist)), HttpGet("me/tracks")]
        public async Task<IActionResult> GetOwnTracks([FromQuery] int offset = 1, [FromQuery] int limit = 10)
        {
            var trackResponseModels = await _artistService.GetOwnTracks(offset, limit);
            return Ok(new { Message = "Get Own Tracks Successfully", trackResponseModels });
        }

        [Authorize(Roles = $"{nameof(UserRole.Artist)},{nameof(UserRole.Customer)}"), HttpGet("{artistId}/profile")]
        public async Task<IActionResult> GetArtistProfileAsync(string artistId)
        {
            var artistProfile = await _artistService.GetArtistByIdAsync(artistId);
            var artistTracks = await _artistService.GetTracksByArtistId(artistId, 1, 10);
            return Ok(new { Message = "Get Artist Successfully", artistProfile, artistTracks});
        }
    }
}
