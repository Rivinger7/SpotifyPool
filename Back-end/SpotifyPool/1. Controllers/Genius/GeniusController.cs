using BusinessLogicLayer.Interface.Microservices_Interface.Genius;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Genius
{
    [Route("api/v1/genius")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class GeniusController(IGenius geniusService) : ControllerBase
    {
        private readonly IGenius _geniusService = geniusService;

        /// <summary>
        /// Uỷ quyền truy cập vào Genius API
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("authorize")]
        public IActionResult Authorize()
        {
            var result = _geniusService.Authorize();
            return Redirect(result);
        }

        /// <summary>
        /// Đổi authorize code lấy access token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SuperAdmin)), HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code not found.");
            }

            var result = await _geniusService.GetAccessToken(code);
            return Ok(new { AccessToken = result });
        }
    }
}
