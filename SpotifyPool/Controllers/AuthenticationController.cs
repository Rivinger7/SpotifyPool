using BCrypt.Net;
using Business_Logic_Layer.BusinessLogic;
using Business_Logic_Layer.Models;
using Data_Access_Layer.DBContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;
using Business_Logic_Layer.Services.JWT;

namespace SpotifyPool.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationBLL _authenticationBLL;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

		public AuthenticationController(AuthenticationBLL authenticationBLL, ILogger<AuthenticationController> logger, IConfiguration configuration)
		{
			_authenticationBLL = authenticationBLL;
			_logger = logger;
			_configuration = configuration;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
		{
			try
			{
				string password = registerModel.Password;
				string confirmedPassword = registerModel.ConfirmedPassword;

                bool isConfirmedPassword = password == confirmedPassword;
                if (!isConfirmedPassword)
                {
                    throw new ArgumentException("Password and Confirmed Password does not matches");
                }

				await _authenticationBLL.CreateAccount(registerModel);
				return Ok(new { message = "Account created successfully" });
			}
			catch (ArgumentException aex)
			{
				if (aex.ParamName == "usernameExists" || aex.ParamName == "emailExists" || aex.ParamName == "phoneNumberExists")
				{
					return Conflict(new { message = aex.Message });
				}
				return BadRequest(new { message = aex.Message });
			}
			catch (Exception ex)
			{
				//Console.WriteLine(ex.ToString());
				_logger.LogError(ex.StackTrace);
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] string email, string token)
        {
            try
            {
                await _authenticationBLL.ActivateAccountByToken(email, token);
                return Ok(new { message = "Confirmed Email Successfully" });
            }
            catch (ArgumentException aex)
            {
                if(aex.ParamName == "ativateAccountFail" || aex.ParamName == "activatedAccount" || aex.ParamName == "notFound" || aex.ParamName == "updateFail")
                {
                    return Conflict(new {message = aex.Message});
                }
                return BadRequest(new {message = aex.Message});
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                var customerModel = await _authenticationBLL.Authenticate(loginModel);
                JWTGenerator jwtGenerator = new JWTGenerator(_configuration);
                string token = jwtGenerator.GenerateJWTToken(customerModel);
                return Ok(new { message = "Login Successfully", customerModel, token });
            }
            catch (ArgumentException aex)
            {
                if (aex.ParamName == "bannedStatus" || aex.ParamName == "inactiveStatus")
                {
                    return Conflict(new { message = aex.Message });
                }
                return BadRequest(new { message = aex.Message });
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                _logger.LogError(ex.StackTrace);
                return StatusCode(500, new { message = "Internal server error"});
            }
        }

        [HttpGet("login-by-google")]
        public Task<IActionResult> LoginByGoogle(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            return Task.FromResult<IActionResult>(Challenge(properties, GoogleDefaults.AuthenticationScheme));
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal == null)
            {
                return Unauthorized();
            }

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims
                .Select(claim => new
                {
                    claim.Type,
                    claim.Value
                });

            // Tạo JWT hoặc xử lý thêm thông tin người dùng tại đây
            // ...
            // Tạo JWT
            // var token = GenerateJwtToken(result.Principal);
            //return Ok(new { token });

            return Ok(new { message = "Google login successful", claims });
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
