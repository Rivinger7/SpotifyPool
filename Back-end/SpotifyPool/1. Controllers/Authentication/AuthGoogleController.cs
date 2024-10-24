using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Authentication
{
    [Route("api/auth-google")]
    [ApiController]
    public class AuthGoogleController(IAuthGoogle authGoogle) : ControllerBase
    {
        private readonly IAuthGoogle _googleAuthService = authGoogle;

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestModel request)
        {
            if (string.IsNullOrEmpty(request.GoogleToken))
            {
                return BadRequest("Google token is missing.");
            }

            var token = await _googleAuthService.AuthenticateGoogleUserAsync(request.GoogleToken);
            return Ok(new { token });
        }
    }
}
