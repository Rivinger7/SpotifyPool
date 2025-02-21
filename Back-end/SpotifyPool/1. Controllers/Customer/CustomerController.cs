using Business_Logic_Layer.Services_Interface.Users;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Customer
{
    [Route("api/v1/customers")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class CustomerController(IUser userBLL, IPlaylist playlistService) : ControllerBase
    {
        private readonly IUser _userBLL = userBLL;
        private readonly IPlaylist _playlistService = playlistService;

        [Authorize(Roles = nameof(UserRole.Customer)), HttpGet("me/profile")]
		public async Task<IActionResult> GetProfileAsync()
		{
			var user = await _userBLL.GetProfileAsync();
			return Ok(user);
		}

		/// <summary>
		/// Chỉnh sửa thông tin cá nhân (Tên, SDT, Sinh nhật, Giới tính, ,Ảnh)
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}"), HttpPatch("me/profile")]
        public async Task<IActionResult> EditProfileAsync([FromForm] EditProfileRequestModel request)
		{
			await _userBLL.EditProfileAsync(request);
			return Ok("Update profile successfully!");
		}

		[Authorize(Roles = nameof(UserRole.Customer)), HttpPost("me/profile-switch")]
        public async Task<IActionResult> SwitchToArtistProfile()
        {
            var authenticatedResponseModel = await _userBLL.SwitchToArtistProfile();
            return Ok(new { Message = "Switch Profile Successfully", authenticatedResponseModel });
        }

        [Authorize(Roles = nameof(UserRole.Customer)), HttpPost("me/playlists")]
		public async Task<IActionResult> GetPlaylistAsync()
		{
            var result = await _playlistService.GetPlaylistsAsync();
            return Ok(result);
        }

  //      /// <summary>
  //      /// Phân trang cho Users
  //      /// </summary>
  //      /// <param name="offset">Trang thứ n</param>
  //      /// <param name="limit">Số lượng phần tử</param>
  //      /// <returns></returns>
  //      [Authorize(Roles = nameof(UserRole.Admin)), HttpGet("get-user-paging")]
		//public async Task<IActionResult> GetUserPagingAsync([FromQuery] int offset = 1, [FromQuery] int limit = 5)
		//{
		//	var users = await _userBLL.TestPaging(offset, limit);
		//	return Ok(users);
		//}
    }
}
