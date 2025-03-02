using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Interface.Microservices_Interface.AWS
{
    public interface IAmazonWebService
    {
        //Task<string> UploadAndConvertToStreamingFile(IFormFile audioFile);
        //Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile);
        Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile, string fileName);
        Task<string> UploadFileAsync(IFormFile audioFile, string trackId);
        Task<string> UploadFolderAsync(string localFolderPath, string trackId, string trackName);
    }
}
