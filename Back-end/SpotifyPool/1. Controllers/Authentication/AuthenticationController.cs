using Microsoft.AspNetCore.Mvc;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;
using BusinessLogicLayer.ModelView;

namespace SpotifyPool.Controllers.Authentication
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController(IAuthenticationBLL authenticationBLL) : ControllerBase
    {
        private readonly IAuthenticationBLL authenticationBLL = authenticationBLL;

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

        [HttpPost("resend-email-comfirm")]
        public async Task<IActionResult> ResendEmailConfirm()
        {
            await authenticationBLL.ReActiveAccountByToken();

            return Ok(new { message = "Email has sent to user's mail" });
        }

        [HttpPost("login-by-google")]
        public async Task<IActionResult> LoginByGoogle([FromBody] GoogleLoginRequestModel googleToken)
        {
            var token = await authenticationBLL.LoginByGoogle(googleToken.GoogleToken);
            return Ok(new { token });
        }

        [HttpPost("confirm-link-with-google-account")]
        public async Task<IActionResult> ConfirmLinkWithGoogleAccount([FromBody] string email)
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            await authenticationBLL.SendOTPForgotPasswordAsync(model);
            return Ok();
        }

        /// <summary>
        /// Note here or comment on swagger
        /// </summary>
        /// <param name="email"></param>
        /// <param name="otpCode"></param>
        /// <returns></returns>
        [HttpPost("confirm-otp")]
        public async Task<IActionResult> ValidateOTP([FromBody] string otpCode, string email) //chỗ này đang ?, ko biết Hòa lấy OTP với cái gì nên đang để tạm
        {
            await authenticationBLL.ConfirmOTP(email, otpCode);
            return Ok(new { message = "Reset password successfully, please check your email to get new password." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            await authenticationBLL.ResetPasswordAsync(model);
            return Ok(new{message = "Reset password successfully"});
        }
    }
}

