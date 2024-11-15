using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Cloudinary
{
    [Route("api/v1/cloudinary")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class CloudinaryController(CloudinaryService cloudinaryService) : ControllerBase
    {
        private readonly CloudinaryService cloudinaryService = cloudinaryService;

        /// <summary>
        /// Lấy thông tin hình ảnh từ Cloudinary
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.SuperAdmin)}"), HttpGet("get-image/{publicID}")]
        public IActionResult GetImageResult(string publicID)
        {
            var getResult = cloudinaryService.GetImageResult(publicID);
            return Ok(new { message = "Get Image Successfully", getResult });
        }

        /// <summary>
        /// Lấy thông tin track từ Cloudinary
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("get-track/{publicID}")]
        public IActionResult GetTrackResult(string publicID)
        {
            var getResult = cloudinaryService.GetTrackResult(publicID);
            return Ok(new { message = "Get Track Successfully", getResult });
        }

        /// <summary>
        /// Kiểm tra Content-Type của file
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.SuperAdmin)}"), HttpPost("test-content-type-file")]
        public IActionResult TestContentTypeFile(IFormFile formFile)
        {
            if (formFile == null)
            {
                return BadRequest("ehhh");
            }

            var contentType = formFile.ContentType;

            string[] type = contentType.Split("/");

            return Ok(new { ContentType = contentType, Type = type });
        }

        /// <summary>
        /// Tải hình ảnh lên Cloudinary
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="imageTag"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.SuperAdmin)}"), HttpPost("upload-image")]
        public IActionResult UploadImage(IFormFile imageFile, ImageTag imageTag)
        {
            var uploadResult = cloudinaryService.UploadImage(imageFile, imageTag);
            return Ok(new { message = "Upload Image Successfully", uploadResult });
        }

        /// <summary>
        /// Tải track lên Cloudinary
        /// </summary>
        /// <param name="trackFile"></param>
        /// <param name="audioTagParent"></param>
        /// <param name="audioTagChild"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.SuperAdmin)}"), HttpPost("upload-track")]
        public IActionResult UploadTrack(IFormFile trackFile, AudioTagParent audioTagParent, AudioTagChild audioTagChild)
        {
            var uploadResult = cloudinaryService.UploadTrack(trackFile, audioTagParent, audioTagChild);
            return Ok(new { message = "Upload Track Successfully", uploadResult });
        }

        /// <summary>
        /// Xóa hình ảnh khỏi Cloudinary
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.SuperAdmin)}"), HttpDelete("delete-image/{publicID}")]
        public IActionResult DeleteImage(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteImage(publicID);
            return Ok(new { message = $"Delete Image Successfully with Public ID {publicID}", deleteResult });
        }

        /// <summary>
        /// Xóa track khỏi Cloudinary
        /// </summary>
        /// <param name="publicID"></param>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.SuperAdmin)}"), HttpDelete("delete-track/{publicID}")]
        public IActionResult DeleteTrack(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteTrack(publicID);
            return Ok(new { message = $"Delete Track Successfully with Public ID {publicID}", deleteResult });
        }
    }
}
