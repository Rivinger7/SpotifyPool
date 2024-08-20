using Business_Logic_Layer.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Business_Logic_Layer.Interface;

namespace SpotifyPool.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationBLL _authenticationBLL;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

		public AuthenticationController(IAuthenticationBLL authenticationBLL, ILogger<AuthenticationController> logger, IConfiguration configuration)
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
        public async Task<IActionResult> ConfirmEmail([FromBody] string token)
        {
            try
            {
                await _authenticationBLL.ActivateAccountByToken(token);
                return Ok(new { message = "Confirmed Email Successfully" });
            }
            catch (ArgumentException aex)
            {
                if (aex.ParamName == "ativateAccountFail" || aex.ParamName == "activatedAccount" || aex.ParamName == "notFound" || aex.ParamName == "updateFail")
                {
                    return Conflict(new { message = aex.Message });
                }
                return BadRequest(new { message = aex.Message });
            }
            catch (Exception ex)
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
                JWT jwtGenerator = new JWT(_configuration);
                string token = jwtGenerator.GenerateJWTToken(customerModel);
                return Ok(new { message = "Login Successfully", customerModel, token });
            }
            catch (ArgumentException aex)
            {
                if (aex.ParamName == "bannedStatus" || aex.ParamName == "inactiveStatus")
                {
                    HttpContext.Session.SetString("Username", loginModel.Username);
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

        [HttpPost("reactive-account")]
        public async Task<IActionResult> ReactiveAccount()
        {
            try
            {
                string? username = HttpContext.Session.GetString("Username");

                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = "No username found in session. Please log in again." });
                }

                await _authenticationBLL.ReActiveAccountByToken(username);

                return Ok(new { message = "Email has sent to user's mail" });
            }
            catch (ArgumentException aex)
            {
                return BadRequest(new { message = aex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("login-by-google")]
        public Task<IActionResult> LoginByGoogle(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            return Task.FromResult<IActionResult>(Challenge(properties, GoogleDefaults.AuthenticationScheme));

            //var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            ////HttpContext.Items["oauthState"] = properties; // Lưu state trong HttpContext.Items
            //return Task.FromResult<IActionResult>(Challenge(properties, GoogleDefaults.AuthenticationScheme));
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal == null)
            {
                return Unauthorized();
            }

            //var claims = result.Principal.Identities.FirstOrDefault()?.Claims.ToList();

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims
                .Select(claim => new
                {
                    claim.Type,
                    claim.Value
                });

            // Trước khi kiểm tra liên kết thì cần xem account đó đã tồn tại chưa

            // Kiểm tra user có liên kết google account chưa
            // Nếu có liên kết thì đăng nhập bình thường
            // Nếu chưa liên kết thì yêu cầu người dùng muốn liên kết hay không
            // Nếu không cho phép liên kết thì không cho người dùng đăng nhập bằng google account vì account đã tồn tại google email rồi
            // Nếu cho phép liên kết confirm liên kết

            // Trường hợp mới đăng nhập google account lần đầu tức là chưa tồn tại tài khoản trong db



            // Tạo JWT token từ các claim của Google
            var jwtGenerator = new JWT(_configuration);
            var token = jwtGenerator.GenerateJWTToken(new CustomerModel
            {
                // Map thông tin từ Google vào model của bạn
                Username = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Role = "Customer", // Hoặc lấy từ claim nếu có
            });

            return Ok(new { message = "Google login successful", claims, token });
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
