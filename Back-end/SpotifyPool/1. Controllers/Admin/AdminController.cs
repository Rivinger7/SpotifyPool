using BusinessLogicLayer.Interface.Services_Interface.Admin;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Admin
{
	[Route("api/admin")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
	public class AdminController : ControllerBase
	{
		private readonly IAdmin _adminBLL;

		public AdminController(IAdmin adminBLL)
		{
			_adminBLL = adminBLL;
		}

		/// <summary>
		/// Hiển thị thông tin tất cả người dùng
		/// </summary>
		/// <param name="request">Trang thứ n và Kích cỡ trang</param>
		/// <param name="model"></param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Admin)), HttpGet()]
		public async Task<IActionResult> GetAllAccount([FromQuery] PagingRequestModel request, [FromQuery] AdminFilter model)
		{
			var customer = await _adminBLL.GetAllAccountAsync(request, model);
			return Ok(customer);
		}

		/// <summary>
		/// Hiển thị đẩy đủ thông tin người dùng
		/// </summary>
		/// <param name="id">Id người dùng</param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Admin)), HttpGet("/{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			var customer = await _adminBLL.GetByIdAsync(id);
			return Ok(customer);
		}

		/// <summary>
		/// Chỉnh sửa thông tin tài khoản người dùng
		/// </summary>
		/// <param name="id">Id người dùng</param>
		/// <param name="userRequest">Thông tin cần chỉnh sửa</param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Admin)), HttpPost("{id}")]
		public async Task<IActionResult> UpdateById(string id, [FromQuery] UpdateUserRequest userRequest)
		{
			await _adminBLL.UpdateByIdAsync(id, userRequest);
			return Ok(new { Message = "Update Account Successfully" });
		}

		/// <summary>
		/// Cấm tài khoản người dùng
		/// </summary>
		/// <param name="id">Id người dùng</param>
		/// <returns></returns>
		[Authorize(Roles = nameof(UserRole.Admin)), HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			await _adminBLL.DeleteByIdAsync(id);
			return Ok(new { Message = "Ban Account Successfully" });
		}
	}
}
