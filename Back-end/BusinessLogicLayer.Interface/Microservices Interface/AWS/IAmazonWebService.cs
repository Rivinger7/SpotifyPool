using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Interface.Microservices_Interface.AWS
{
    public interface IAmazonWebService
    {
        Task<string> CreateHlsJobAsync(string inputFile);
        Task<string> UploadAndConvertToStreamingFile(IFormFile audioFile);
        Task UploadAudioFileAsync(IFormFile audioFile);
    }
}
