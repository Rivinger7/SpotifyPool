using Business_Logic_Layer.Services_Interface.InMemoryCache;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool.Controllers.Media
{
    [Route("api/media")]
    [ApiController]
    public class MediaController(CloudinaryService cloudinaryService) : ControllerBase
    {
        private readonly CloudinaryService cloudinaryService = cloudinaryService;

        [HttpPost("upload-image")]
        public IActionResult UploadImage(IFormFile imageFile)
        {
            var uploadResult = cloudinaryService.UploadImage(imageFile);
            return Ok(new { message = "Upload Image Successfully", uploadResult });
        }

        [HttpPost("upload-video")]
        public IActionResult UploadVideo(IFormFile videoFile)
        {
            var uploadResult = cloudinaryService.UploadVideo(videoFile);
            return Ok(new { message = "Upload Video Successfully", uploadResult });
        }

        [HttpGet("get-image/{publicID}")]
        public IActionResult GetImageResult(string publicID)
        {
            var getResult = cloudinaryService.GetImageResult(publicID);
            return Ok(new { message = "Get Image Successfully", getResult });
        }

        [HttpGet("get-video/{publicID}")]
        public IActionResult GetVideoResult(string publicID)
        {
            var getResult = cloudinaryService.GetVideoResult(publicID);
            return Ok(new { message = "Get Video Successfully", getResult });
        }

        [HttpDelete("delete-image/{publicID}")]
        public IActionResult DeleteImage(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteImage(publicID);
            return Ok(new { message = $"Delete Image Successfully with Public ID {publicID}", deleteResult });
        }

        [HttpDelete("delete-video/{publicID}")]
        public IActionResult DeleteVideo(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteVideo(publicID);
            return Ok(new { message = $"Delete Video Successfully with Public ID {publicID}", deleteResult });
        }
    }
}
