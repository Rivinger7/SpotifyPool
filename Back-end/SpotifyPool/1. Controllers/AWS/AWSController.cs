using BusinessLogicLayer.Interface.Microservices_Interface.AWS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.AWS
{
    [Route("api/v1/aws")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class AWSController(IAmazonWebService amazonWebService) : ControllerBase
    {
        private readonly IAmazonWebService _amazonWebService = amazonWebService;

        [AllowAnonymous, HttpPost("media-jobs")]
        public async Task<IActionResult> UploadAudioFileAsync(IFormFile audioFile, string fileName)
        {
            if (audioFile == null)
            {
                return BadRequest("Audio file not found.");
            }
            var result = await _amazonWebService.UploadAndConvertToStreamingFile(audioFile, fileName);
            return Ok(result);
        }

        //[AllowAnonymous, HttpPost("upload/folder")]
        //public async Task<IActionResult> UploadFolderAsync(string folderPath)
        //{
        //    await _amazonWebService.UploadFolderAsync(folderPath);
        //    return Ok();
        //}
    }
}
