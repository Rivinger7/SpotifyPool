using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;
using BusinessLogicLayer.ModelView;

namespace SpotifyPool.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController(IAuthenticationBLL authenticationBLL, CloudinaryService cloudinaryService) : ControllerBase
    {
        private readonly IAuthenticationBLL authenticationBLL = authenticationBLL;
        private readonly CloudinaryService cloudinaryService = cloudinaryService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel registerModel)
        {
            await authenticationBLL.CreateAccount(registerModel);
            return Ok(new { message = "Account created successfully" });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] string token)
        {
            await authenticationBLL.ActivateAccountByToken(token);
            return Ok(new { message = "Confirmed Email Successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginModel)
        {
            var authenticatedResponseModel = await authenticationBLL.Authenticate(loginModel);
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
        }

        [HttpPost("reactive-account")]
        public async Task<IActionResult> ReactiveAccount()
        {
            string? username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "No username found in session. Please log in again." });
            }

            await authenticationBLL.ReActiveAccountByToken(username);

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
            var authenticatedResponseModel = await authenticationBLL.LoginByGoogle();
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
            //return Redirect(returnUrl); // Tự động chuyển hướng đến returnURL không cần phải nhờ FE chuyển FE chỉ cần bỏ URL vào biến returnURL
        }

        [HttpPost("confirm-link-with-google-account")]
        public async Task<IActionResult> ConfirmLinkWithGoogleAccount([FromBody] string email, [FromQuery] string returnUrl = "/")
        {
            var authenticatedResponseModel = await authenticationBLL.ConfirmLinkWithGoogleAccount(email);
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
            //return Redirect(returnUrl); // Tự động chuyển hướng đến returnURL không cần phải nhờ FE chuyển FE chỉ cần bỏ URL vào biến returnURL
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }


        // Testing
        [HttpPost("upload-image")]
        public IActionResult UploadImage(IFormFile imageFile)
        {
            var uploadResult = cloudinaryService.UploadImage(imageFile);
            return Ok(new { message = "Upload Image Successfully", uploadResult });
        }

        [HttpPost("upload-video")]
        public IActionResult UploadVideo(IFormFile videoFile)
        {
            var uploadResult = cloudinaryService.UploadVideo(videoFile);
            return Ok(new { message = "Upload Video Successfully", uploadResult });
        }

        [HttpGet("get-image/{publicID}")]
        public IActionResult GetImageResult(string publicID)
        {
            var getResult = cloudinaryService.GetImageResult(publicID);
            return Ok(new { message = "Get Image Successfully", getResult });
        }

        [HttpGet("get-video/{publicID}")]
        public IActionResult GetVideoResult(string publicID)
        {
            var getResult = cloudinaryService.GetVideoResult(publicID);
            return Ok(new { message = "Get Video Successfully", getResult });
        }

        [HttpDelete("delete-image/{publicID}")]
        public IActionResult DeleteImage(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteImage(publicID);
            return Ok(new { message = $"Delete Image Successfully with Public ID {publicID}", deleteResult });
        }

        [HttpDelete("delete-video/{publicID}")]
        public IActionResult DeleteVideo(string publicID)
        {
            var deleteResult = cloudinaryService.DeleteVideo(publicID);
            return Ok(new { message = $"Delete Video Successfully with Public ID {publicID}", deleteResult });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            var token = await authenticationBLL.SendTokenForgotPasswordAsync(model);
            return Ok(new { message = "Success! Please check message in your mail.", token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            await authenticationBLL.ResetPasswordAsync(model);
            return Ok("Reset password successfully");
        }
    }
}

