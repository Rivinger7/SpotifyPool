using BusinessLogicLayer.Interface.Services_Interface.Albums;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.Album;
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
		/// Lấy list album (với chỉ những info cơ bản))
		/// </summary>
		/// <param name="paging">Info paging</param>
		/// <param name="model">search hoặc sort tùy yêu cầu</param>
		/// <returns></returns>
		[AllowAnonymous, HttpGet()]
        public async Task<IActionResult> GetAlbumsAsync( [FromQuery] PagingRequestModel paging, [FromQuery] AlbumFilterModel model)
        {
            var result = await _albumsService.GetAlbumsAsync(paging, model);
            return Ok(result);
        }

        /// <summary>
		/// Lấy thông tin chi tiết album (bao gồm list tracks)
		/// </summary>
		/// <param name="id">Id của clbum cần xem</param>
		/// <param name="isSortByTrackName">Sắp xếp list tracks theo tên. true: tăng dần, false: giảm dần.</param>
		/// <returns></returns>
		[AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetAlbumDetailByIdAsync(string id, bool? isSortByTrackName)
        {
            var result = await _albumsService.GetAbumDetailByIdAsync(id, isSortByTrackName);
            return Ok(result);
        }

        /// <summary>
        /// Tạo album mới
        /// </summary>
        /// <param name="albumRequestModel"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpPost()]
        public async Task<IActionResult> CreateAlbumAsync([FromForm] AlbumRequestModel albumRequestModel)
        {
            await _albumsService.CreateAlbumAsync(albumRequestModel);
            return Ok(new { Message = "Create Album Successfully" });
        }

        /// <summary>
        /// Cập nhật thông tin cơ bản album
        /// </summary>
        /// <param name="id"> Id album cần chỉnh sửa</param>
        /// <param name="albumRequestModel"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlbumAsync(string id, [FromForm] AlbumRequestModel albumRequestModel)
        {
            await _albumsService.UpdateAlbumAsync(id, albumRequestModel);
            return Ok(new { Message = "Update Album Successfully" });
        }

        /// <summary>
        /// Xóa album
        /// </summary>
        /// <param name="id"> Id album cần xóa</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbumAsync(string id)
        {
            await _albumsService.DeleteAlbumAsync(id);
            return Ok(new { Message = "Delete Album Successfully" });
        }

        /// <summary>
        /// Thêm track vào album
        /// </summary>
        /// <param name="id">Id của album cần thêm track</param>
        /// <param name="trackIds">list track đc thêm vào album, tự động bỏ qua những track đã tồn tại</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpPost("{id}/tracks")]
        public async Task<IActionResult> AddTracksToAlbum(string id, [FromForm] IEnumerable<string> trackIds)
        {
            await _albumsService.AddTracksToAlbum(trackIds, id);
            return Ok(new { Message = "Add New Tracks to Album Successfully" });
        }

        /// <summary>
        /// Xóa track khỏi album
        /// </summary>
        /// <param name="id">Id của album cần thêm track</param>
        /// <param name="trackIds">list track bị xóa khỏi album</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpDelete("{id}/tracks")]
        public async Task<IActionResult> RemoveTracksFromAlbum(string id, [FromForm] IEnumerable<string> trackIds)
        {
            await _albumsService.RemoveTracksFromAlbum(trackIds, id);
            return Ok(new { Message = "Remove Tracks from Album Successfully" });
        }

        /// <summary>
        /// Đặt lịch phát hành của album (set time và set Reason thành Official)
        /// </summary>
        /// <param name="id"> Id album cần đặt lịch phát hành </param>
        /// <param name="releaseTime">Phát hành ngay (time now) OR hẹn time > now</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpPut("{id}/release-schedule")]
        public async Task<IActionResult> ReleaseAlbum(string id, [FromForm] DateTime releaseTime)
        {
            await _albumsService.ReleaseAlbumAsync(id, releaseTime);
            return Ok(new { Message = "Album release schedule successful." });
        }
        /// <summary>
        /// Thay đổi status album (dùng cho TH ko release đúng time vì delay, cancel or Leaked) 
        /// </summary>
        /// <param name="id"> Id album cần đặt lịch phát hành </param>
        /// <param name="status">Delayed, Canceled, or Leaked</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.Artist)), HttpPut("{id}/status-setting")]
        public async Task<IActionResult> ChangeAlbumStatus(string id, [FromForm] ReleaseStatus status)
        {
            await _albumsService.ChangeAlbumStatusAsync(id, status);
            return Ok(new { Message = "Change album status successfully." });
        }
    }
}
