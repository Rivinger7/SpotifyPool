using BusinessLogicLayer.Implement.Services.Files;
using BusinessLogicLayer.Implement.Services.Tests;
using BusinessLogicLayer.Interface.Services_Interface.Files;
using Flurl.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;
using SharpCompress.Common;

namespace SpotifyPool._1._Controllers.Tests
{
	[Route("api/test")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
	public class TestController(TestBLL testBLL) : ControllerBase
	{
		[AllowAnonymous, HttpPost("test-naudio")]
        public async Task<IActionResult> TestNAudio(IFormFile audioFile)
        {
            await testBLL.TestNAudioService(audioFile);
            return Ok();
        }

        //       private readonly IFiles _fileService = new FilesBLL();
        //	//[AllowAnonymous, HttpPatch("Testing-Set-Artist-Account")]
        //	//public async Task<IActionResult> TestingSetArtistAccount()
        //	//{
        //	//    await testBLL.SetArtistAccount();
        //	//    return Ok();
        //	//}

        //       [AllowAnonymous, HttpGet("Testing-Pallete")]
        //	public async Task<IActionResult> TestingPallete()
        //	{
        //		IEnumerable<string> colors = await TestBLL.TestImgx();

        //		return Ok(colors);
        //	}

        //	[AllowAnonymous, HttpGet("Testing-file-path")]
        //	public IActionResult TestingFilePath()
        //	{
        //		string[] filePaths = testBLL.GetFilePath();
        //		return Ok(filePaths);
        //	}

        //	[Authorize(Roles = nameof(UserRole.Admin)), HttpGet("Testing-Date")]
        //	public async Task<IActionResult> TestingDate()
        //	{
        //		(string addedAtString, DateTime addedAtTime) = await testBLL.AddDayOnly();
        //		return Ok(new { addedAtString, addedAtTime });
        //	}

        //	//[AllowAnonymous, HttpGet("Testing-Lyrics")]
        //	//public async Task<IActionResult> TestingLyrics(string trackName, string artistName)
        //	//{
        //	//    string? lyrics = await testBLL.GetLyricsAsync(trackName, artistName);
        //	//    return Ok(lyrics);
        //	//}


        //	//[AllowAnonymous, HttpGet("Testing-Set-Lyrics")]
        //	//public async Task<IActionResult> TestingSetLyrics()
        //	//{
        //	//    await testBLL.SetLyricsToDatabase();
        //	//    return Ok();
        //	//}


        //	// [AllowAnonymous, HttpGet("Testing-Set-Lyrics")]
        //	// public async Task<IActionResult> TestingSetLyrics()
        //	// {
        //	//     await testBLL.SetLyricsToDatabase();
        //	//     return Ok();
        //	// }

        //	[AllowAnonymous, HttpGet("test-top-track")]
        //	public async Task<IActionResult> TestTopTrack(string trackId)
        //	{
        //		await testBLL.TestTopTrack(trackId);
        //		return Ok();
        //	}

        //	[AllowAnonymous, HttpPost("test-upload-to-bunny")]
        //	public async Task<IActionResult> UploadFile(List<IFormFile> files)
        //	{
        //		string ApiKey = "1f894c77-2952-4ca0-bf54938d60c8-387f-4c08";  //AccessKey password
        //		string BunnyStorageUrl = "https://storage.bunnycdn.com";
        //		string StorageZoneName = "spotifypool-storage";
        //		string folder = "test";
        //		if (files == null || files.Count == 0)
        //			return BadRequest("File không hợp lệ");

        //		List<object> results = new List<object>();

        //		try
        //		{
        //			foreach (IFormFile file in files)
        //			{
        //				if (file.Length == 0)
        //					continue; //Bỏ qua file rỗng

        //				string fileName = Path.GetFileName(file.FileName);
        //				string url = $"{BunnyStorageUrl}/{StorageZoneName}/{folder}/{fileName}";

        //				using (Stream stream = file.OpenReadStream())
        //				using (StreamContent content = new StreamContent(stream)) //Chuyển Stream thành HttpContent
        //				{
        //					IFlurlResponse response = await url
        //						.WithHeader("AccessKey", ApiKey)
        //						.PutAsync(content);

        //					// Kiểm tra nếu status code là 2xx thì thành công
        //					if (response.StatusCode >= 200 && response.StatusCode < 300)
        //					{
        //						results.Add(new { fileName, url, status = "Upload thành công" });
        //					}
        //					else
        //					{
        //						results.Add(new { fileName, status = "Upload thất bại", errorCode = response.StatusCode });
        //					}
        //				}
        //			}
        //			return Ok(new { message = "Upload hoàn tất", files = results });
        //		}
        //		catch (Exception ex)
        //		{
        //			return StatusCode(500, $"Lỗi: {ex.Message}");
        //		}
        //	}

        //       [AllowAnonymous, HttpPost("test-upload-FilesBll")]
        //       public async Task<IActionResult> CheckUploadFilesBll(IFormFile file)
        //       {
        //           string fileName = Path.GetFileName(file.FileName);
        //		string folder = "test";
        //           string? fileUrl = await _fileService.UploadFile(file, fileName, folder);
        //           return Ok(new { message = "Upload hoàn tất", fileUrl });
        //       }

        //       [AllowAnonymous, HttpDelete("test-delete-FilesBll")]
        //       public async Task<IActionResult> CheckDeleteFilesBll(string path)
        //       {
        //		bool isDeleted = await _fileService.DeleteFile(path);
        //           return Ok(new { message = "Đã xóa: ", path });
        //       }
        //   }

        //[AllowAnonymous, HttpDelete("test-download-FilesBll")]
        //public async Task<IActionResult> CheckDownloadFilesBll(string path)
        //{
        //    try
        //    {
        //        byte[] fileBytes = await _fileService.DownloadFile(path);

        //        if (fileBytes == null || fileBytes.Length == 0)
        //        {
        //            return NotFound("File không tồn tại hoặc không thể tải xuống.");
        //        }

        //        string contentType = "application/octet-stream"; // Hoặc xác định loại file dựa trên extension
        //        string fileName = Path.GetFileName(path); // Lấy tên file từ đường dẫn

        //        return File(fileBytes, contentType, fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Lỗi khi tải file: {ex.Message}");
        //    }
    }
}
