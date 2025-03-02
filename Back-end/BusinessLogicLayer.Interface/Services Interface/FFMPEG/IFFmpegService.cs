using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Interface.Services_Interface.FFMPEG
{
    public interface IFFmpegService
    {
        Task<(string, string)> ConvertToHls(IFormFile audioFile, string trackId);
    }
}
