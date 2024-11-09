using BusinessLogicLayer.Implement.Services.Tests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Tests
{
    [Route("api/test")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class TestController(TestBLL testBLL) : ControllerBase
    {

        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("Testing-Date")]
        public async Task<IActionResult> TestingDate()
        {
            (string addedAtString, DateTime addedAtTime) = await testBLL.AddDayOnly();
            return Ok(new { addedAtString, addedAtTime });
        }
    }
}
