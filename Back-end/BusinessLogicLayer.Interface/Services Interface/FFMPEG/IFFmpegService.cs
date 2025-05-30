using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Interface.Services_Interface.FFMPEG
{
    public interface IFFmpegService : IDisposable
    {
        Task<(string, string, string)> ConvertToHls(IFormFile audioFile, string trackId);
        Task<string> ConvertToHlsTemp(string audioFilePath, string trackId, string? basePath, string? rootFolder, string? outputIntermediateFolder, string? targetFolder = null);
        Task<(string, long)> ConvertToWavFileAsync(IFormFile inputFile, string? basePath, string? rootFolder, string? inputIntermediateFolder, string? ouputIntermediateFolder);
        void DeleteFileAsync(string filePath);
    }
}
