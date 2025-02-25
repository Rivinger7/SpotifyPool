using BusinessLogicLayer.Interface.Services_Interface.Albums;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Album
{
    [Route("api/v1/albums")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    [Authorize(Roles = nameof(UserRole.Artist))]
    public class AlbumController(IAlbums albumsService) : Controller
    {
        private readonly IAlbums _albumsService = albumsService;

        /// <summary>
        /// Tạo album mới
        /// </summary>
        /// <param name="albumRequestModel"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<IActionResult> CreateAlbumAsync([FromForm] AlbumRequestModel albumRequestModel)
        {
            await _albumsService.CreateAlbumAsync(albumRequestModel);
            return Ok(new { Message = "Create Album Successfully" });
        }

        /// <summary>
        /// Cập nhật thông album
        /// </summary>
        /// <param name="id"> Id album cần chỉnh sửa</param>
        /// <param name="albumRequestModel"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlbumAsync(string id, [FromForm] AlbumRequestModel albumRequestModel)
        {
            await _albumsService.UpdateAlbumAsync(id, albumRequestModel);
            return Ok(new { Message = "Update Album Successfully" });
        }

        /// <summary>
        /// Cập nhật thông album
        /// </summary>
        /// <param name="id"> Id album cần chỉnh sửa</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbumAsync(string id)
        {
            await _albumsService.DeleteAlbumAsync(id);
            return Ok(new { Message = "Delete Album Successfully" });
        }
    }
}
