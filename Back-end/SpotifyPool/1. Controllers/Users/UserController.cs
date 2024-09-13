using Business_Logic_Layer.Services_Interface.Users;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool._1._Controllers.Users
{
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserBLL userBLL) : ControllerBase
    {
        private readonly IUserBLL _userBLL = userBLL;

        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] string? fullname, [FromQuery] string? gender, [FromQuery] string? email)
        {
            var users = await _userBLL.GetAllUsersAsync(fullname, gender, email);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIDAsync(string id)
        {
            var user = await _userBLL.GetUserByIDAsync(id, true);
            return Ok(user);
        }
    }
}
