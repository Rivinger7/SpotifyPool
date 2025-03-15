using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Interface.Services_Interface.FFMPEG
{
    public interface IFFmpegService : IDisposable
    {
        Task<(string, string, string)> ConvertToHls(IFormFile audioFile, string trackId);
        Task<(string, string, string)> ConvertToHlsTemp(IFormFile audioFile, string trackId);
    }
}
