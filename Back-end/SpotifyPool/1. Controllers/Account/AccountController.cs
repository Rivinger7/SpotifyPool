using BusinessLogicLayer.Interface.Services_Interface.Admin;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Account
{
	[Route("api/v1/accounts")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
	[Authorize(Roles = nameof(UserRole.Admin))]
	public class AccountController(IAdmin adminBLL) : ControllerBase
	{
		private readonly IAdmin _adminBLL = adminBLL;

		/// <summary>
		/// Hiển thị danh sách tài khoản ngưởi dùng
		/// </summary>
		/// <param name="request">Trang thứ n và Kích cỡ trang</param>
		/// <param name="model">Lọc</param>
		/// <returns>
		/// </returns>
		[HttpGet()]
		public async Task<IActionResult> GetListAccounts([FromQuery] PagingRequestModel request, [FromQuery] AdminFilter model)
		{
			var customer = await _adminBLL.GetPaggingAsync(request, model);
			return Ok(customer);
		}

		/// <summary>
		/// Chi tiết thông tin tài khoản 
		/// </summary>
		/// <param name="id">Id người dùng</param>
		/// <returns></returns>
		[HttpGet("{id}")]
		public async Task<IActionResult> GetAccountById(string id)
		{
			var customer = await _adminBLL.GetByIdAsync(id);
			return Ok(customer);
		}

		/// <summary>
		/// Tạo tài khoản người dùng
		/// </summary>
		/// <param name="model">Thông tin người dùng cần tạo</param>
		/// <returns></returns>
		[HttpPost()]
		public async Task<IActionResult> CreateAccount([FromQuery] CreateRequestModel model)
		{
			await _adminBLL.CreateAsync(model);
			return Ok(new { Message = "Create Account Successfully" });
		}

		/// <summary>
		/// Chỉnh sửa thông tin tài khoản người dùng
		/// </summary>
		/// <param name="id">Id người dùng</param>
		/// <param name="userRequest">Thông tin cần chỉnh sửa</param>
		/// <returns></returns>
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAccountById(string id, [FromQuery] UpdateRequestModel userRequest)
		{
			await _adminBLL.UpdateByIdAsync(id, userRequest);
			return Ok(new { Message = "Update Account Successfully" });
		}

		/// <summary>
		/// Cấm tài khoản người dùng
		/// </summary>
		/// <param name="id">Id người dùng</param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		public async Task<IActionResult> BanAccount(string id)
		{
			await _adminBLL.DeleteByIdAsync(id);
			return Ok(new { Message = "Ban Account Successfully" });
		}
	}
}
