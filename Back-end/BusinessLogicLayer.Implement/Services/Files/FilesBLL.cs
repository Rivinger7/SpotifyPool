using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Files;
using Flurl.Http;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Implement.Services.Files
{
    public class FilesBLL : IFiles
    {
        //private string ApiKey = Environment.GetEnvironmentVariable("BunnyCDN_API_KEY")
        //    ?? throw new DataNotFoundCustomException("BunnyCDN ApiKey property is not set in environment or not found");
        //private string BunnyStorageZoneName = Environment.GetEnvironmentVariable("BunnyCDN_StorageZoneName")
        //    ?? throw new DataNotFoundCustomException("BunnyCDN StorageZoneName property is not set in environment or not found");

        //public async Task<string?> UploadFile(IFormFile file, string fileName, string folderName)
        //{
        //    string url = $"https://storage.bunnycdn.com/{BunnyStorageZoneName}/{folderName}/{fileName}";
        //    if (file == null || file.Length == 0)
        //        throw new InvalidDataCustomException("Invalid file input.");
        //    try
        //    {
        //        using (var stream = file.OpenReadStream())
        //        using (var content = new StreamContent(stream)) // Chuyển Stream thành HttpContent
        //        {
        //            IFlurlResponse response = await url
        //                .WithHeader("AccessKey", ApiKey)
        //                .PutAsync(content);

        //            // Kiểm tra nếu status code là 2xx thì thành công
        //            if (response.StatusCode >= 200 && response.StatusCode < 300)
        //            {
        //                return url; // Trả về đường dẫn file trên BunnyCDN
        //            }

        //            // Nếu lỗi HTTP
        //            throw new BadRequestCustomException(response.StatusCode, "Upload thất bại");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BadRequestCustomException(500, $"Error: {ex.Message}");
        //    }
        //}

        //public async Task<bool> DeleteFile(string path)
        //{
        //    try
        //    {
        //        IFlurlResponse response = await path
        //            .WithHeader("AccessKey", ApiKey)
        //            .AllowHttpStatus(404) // Cho phép xử lý nếu file không tồn tại
        //            .DeleteAsync();

        //        // Kiểm tra nếu status code
        //        if (response.StatusCode >= 200 && response.StatusCode < 300 || response.StatusCode == 404)
        //        {
        //            return true; // Xóa thành công hoặc nó ko tồn tại sẵn rồi
        //        }

        //        // Nếu lỗi HTTP
        //        throw new BadRequestCustomException(response.StatusCode, "Delete thất bại");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BadRequestCustomException(500, $"Error: {ex.Message}");
        //    }
        //}

        //public async Task<byte[]> DownloadFile(string path)
        //{
        //    try
        //    {
        //        IFlurlResponse response = await path
        //            .WithHeader("AccessKey", ApiKey)
        //            .GetAsync();

        //        // Kiểm tra nếu status code thành công
        //        if (response.StatusCode >= 200 && response.StatusCode < 300)
        //        {
        //            return await response.GetBytesAsync(); // Trả về nội dung file dưới dạng byte[]
        //        }

        //        // Nếu lỗi HTTP
        //        throw new BadRequestCustomException(response.StatusCode, "Download thất bại");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BadRequestCustomException(500, $"Error: {ex.Message}");
        //    }
        //}

    }
}
