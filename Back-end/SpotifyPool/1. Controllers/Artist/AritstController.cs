using BusinessLogicLayer.Interface.Services_Interface.Artists;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Artist
{
    [Route("api/artists")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class AritstController(IArtist artistService) : ControllerBase
    {
        private readonly IArtist _artistService = artistService;

        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("register")]
        public async Task<IActionResult> CreateArtist([FromForm] ArtistRequest artistRequest)
        {
            await _artistService.CreateArtist(artistRequest);
            return Ok(new { Message = "Create Artist Successfully" });
        }

        [Authorize(Roles = nameof(UserRole.Artist)), HttpPost("switch-profile")]
        public async Task<IActionResult> SwitchToUserProfile()
        {
            var authenticatedResponseModel = await _artistService.SwitchToUserProfile();
            return Ok(new { Message = "Switch Profile Successfully", authenticatedResponseModel });
        }
    }
}
