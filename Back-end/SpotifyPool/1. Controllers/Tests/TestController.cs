using BusinessLogicLayer.DataAnalytics;
using BusinessLogicLayer.Implement.Services.Tests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Tests
{
    [Route("api/test")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class TestController(TestBLL testBLL) : ControllerBase
    {
        [AllowAnonymous, HttpGet("Testing-Spectrogram")]
        public async Task<IActionResult> TestingSpectrogram()
        {
            await testBLL.TestSpectrogram();
            return Ok();
        }

        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("Testing-Date")]
        public async Task<IActionResult> TestingDate()
        {
            (string addedAtString, DateTime addedAtTime) = await testBLL.AddDayOnly();
            return Ok(new { addedAtString, addedAtTime });
        }

        //[AllowAnonymous, HttpGet("Testing-Lyrics")]
        //public async Task<IActionResult> TestingLyrics(string trackName, string artistName)
        //{
        //    string? lyrics = await testBLL.GetLyricsAsync(trackName, artistName);
        //    return Ok(lyrics);
        //}


        //[AllowAnonymous, HttpGet("Testing-Set-Lyrics")]
        //public async Task<IActionResult> TestingSetLyrics()
        //{
        //    await testBLL.SetLyricsToDatabase();
        //    return Ok();
        //}


        // [AllowAnonymous, HttpGet("Testing-Set-Lyrics")]
        // public async Task<IActionResult> TestingSetLyrics()
        // {
        //     await testBLL.SetLyricsToDatabase();
        //     return Ok();
        // }

        [AllowAnonymous,HttpGet("test-top-track")]
        public async Task<IActionResult> TestTopTrack(string trackId)
        {
            await testBLL.TestTopTrack(trackId);
            return Ok();
        }

	}
}
