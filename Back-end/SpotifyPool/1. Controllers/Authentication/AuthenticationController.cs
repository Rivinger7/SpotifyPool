using Business_Logic_Layer.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
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
            await _authenticationBLL.CreateAccount(registerModel);
            return Ok(new { message = "Account created successfully" });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] string token)
        {
            await _authenticationBLL.ActivateAccountByToken(token);
            return Ok(new { message = "Confirmed Email Successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var authenticatedResponseModel = await _authenticationBLL.Authenticate(loginModel);
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
        }

        [HttpPost("reactive-account")]
        public async Task<IActionResult> ReactiveAccount()
        {
            string? username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "No username found in session. Please log in again." });
            }

            await _authenticationBLL.ReActiveAccountByToken(username);

            return Ok(new { message = "Email has sent to user's mail" });
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
            var authenticatedResponseModel = await _authenticationBLL.LoginByGoogle();
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
            //return Redirect(returnUrl); // Tự động chuyển hướng đến returnURL không cần phải nhờ FE chuyển FE chỉ cần bỏ URL vào biến returnURL

        }

        [HttpPost("confirm-link-with-google-account")]
        public async Task<IActionResult> ConfirmLinkWithGoogleAccount([FromBody] string email, [FromQuery] string returnUrl = "/")
        {
            var authenticatedResponseModel = await _authenticationBLL.ConfirmLinkWithGoogleAccount(email);
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
            //return Redirect(returnUrl); // Tự động chuyển hướng đến returnURL không cần phải nhờ FE chuyển FE chỉ cần bỏ URL vào biến returnURL
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
