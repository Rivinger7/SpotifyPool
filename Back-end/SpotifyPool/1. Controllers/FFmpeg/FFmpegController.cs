using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.FFmpeg
{
    [Route("api/[controller]")]
    [ApiController]
    public class FFmpegController(IFFmpegService fFmpegService) : ControllerBase
    {
        private readonly IFFmpegService _fFmpegService = fFmpegService;

        [HttpPost("convert")]
        public async Task<IActionResult> Convert(IFormFile audioFile, [FromQuery] string trackId)
        {
            var result = await _fFmpegService.ConvertToHlsTemp(audioFile, trackId);
            return Ok(result);
        }
    }
}
