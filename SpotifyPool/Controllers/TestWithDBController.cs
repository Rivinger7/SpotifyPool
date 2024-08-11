using Microsoft.AspNetCore.Mvc;
using SpotifyPool.Data;
using SpotifyPool.Services;

namespace SpotifyPool.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TestWithDBController : ControllerBase
	{
		private readonly TestUserWithDBServices _testWithDBServices;
		public TestWithDBController(TestUserWithDBServices testWithDBServices)
		{
			_testWithDBServices = testWithDBServices;
		}

		[HttpGet("GetAllUser")]
		public async Task<List<User>> GetAllUser() => await _testWithDBServices.GetAllUser();

		[HttpPost("register")]
		public async Task< IActionResult> RegisterUser(string username, string password, string email, string birthdate)
		{
			if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
			{
				User user = new User
				{
					Username = username,
					Password = password,
					Email = email,
					Birthdate = birthdate
				};
				await _testWithDBServices.RegisterUser(user);
				return Ok(user);
			}
			return BadRequest();
		}

		[HttpPost("login")]
		public async Task<IActionResult> LoginUser(string username, string password)
		{
			User user = await _testWithDBServices.LoginUser(username, password);
			if(user != null)
			{
				return Ok(user);
			}
			return BadRequest();
		}
	}
}
