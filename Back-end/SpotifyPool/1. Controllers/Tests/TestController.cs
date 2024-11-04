using BusinessLogicLayer.Implement.Services.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Tests
{
    [Route("api/test")]
    [ApiController]
    public class TestController(TestBLL testBLL) : ControllerBase
    {

        [HttpGet("Testing-Date")]
        public async Task<IActionResult> TestingDate()
        {
            (string addedAtString, DateTime addedAtTime) = await testBLL.AddDayOnly();
            return Ok(new { addedAtString, addedAtTime });
        }
    }
}
