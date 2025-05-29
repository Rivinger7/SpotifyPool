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
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string rootFolder = "audio_processing";
            string inputIntermediateFolder = "input_temp_audio";
            string ouputIntermediateFolder1 = "output_wav_audio";
            string ouputIntermediateFolder2 = "output_hls_audio";

            (string audioFilePath, long? bitrate) = await _fFmpegService.ConvertToWavFileAsync(audioFile, basePath, rootFolder, inputIntermediateFolder, ouputIntermediateFolder1);

            var result = await _fFmpegService.ConvertToHlsTemp(audioFilePath, trackId, basePath, rootFolder, ouputIntermediateFolder2);

            _fFmpegService.DeleteFileAsync(audioFilePath);

            return Ok(new { Result = result, Bitrate = bitrate });
        }
    }
}
