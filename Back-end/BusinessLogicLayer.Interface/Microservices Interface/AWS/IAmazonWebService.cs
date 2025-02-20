using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Interface.Microservices_Interface.AWS
{
    public interface IAmazonWebService
    {
        //Task<string> UploadAndConvertToStreamingFile(IFormFile audioFile);
        //Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile);
        Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile, string fileName);
    }
}
