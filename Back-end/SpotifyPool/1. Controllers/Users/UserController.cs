using Business_Logic_Layer.Services_Interface.Users;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Users
{
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserBLL userBLL) : ControllerBase
    {
        private readonly IUserBLL _userBLL = userBLL;

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="gender"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] string? fullname, [FromQuery] string? gender, [FromQuery] string? email)
        {
            var users = await _userBLL.GetAllUsersAsync(fullname, gender, email);
            return Ok(users);
        }

        /// <summary>
        /// FOR BACK-END
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIDAsync(string id)
        {
            var user = await _userBLL.GetUserByIDAsync(id, true);
            return Ok(user);
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfileAsync([FromForm] EditProfileRequestModel request)
		{
			await _userBLL.EditProfileAsync(request);
			return Ok("Update profile successfully!");
		}

        /// <summary>
        /// Phân trang cho Users
        /// </summary>
        /// <param name="offset">Trang thứ n</param>
        /// <param name="limit">Số lượng phần tử</param>
        /// <returns></returns>
        [HttpGet("get-user-paging")]
		public async Task<IActionResult> GetUserPagingAsync([FromQuery] int offset, [FromQuery] int limit)
		{
			var users = await _userBLL.TestPaging(offset, limit);
			return Ok(users);
		}
    }
}
