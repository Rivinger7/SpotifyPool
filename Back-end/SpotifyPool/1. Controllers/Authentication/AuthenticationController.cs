using Microsoft.AspNetCore.Mvc;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;
using BusinessLogicLayer.ModelView;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using SetupLayer.Enum.Services.User;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView.Service_Model_Views.JWTs.Request;

namespace SpotifyPool.Controllers.Authentication
{
    [Route("api/v1/authentication")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class AuthenticationController(IAuthentication authenticationBLL, IJwtBLL jwtBLL) : ControllerBase
    {
        private readonly IAuthentication authenticationBLL = authenticationBLL;
        private readonly IJwtBLL _jwtBLL = jwtBLL;

        [AllowAnonymous, HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel registerModel)
        {
            await authenticationBLL.CreateAccount(registerModel);
            return Ok(new { message = "Account created successfully" });
        }

        [AllowAnonymous, HttpPost("email-confirmation")]
        public async Task<IActionResult> ConfirmEmail([FromBody] string token)
        {
            await authenticationBLL.ActivateAccountByToken(token);
            return Ok(new { message = "Confirmed Email Successfully" });
        }

        [AllowAnonymous, HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginModel)
        {
            var accessToken = await authenticationBLL.Authenticate(loginModel);
            return Ok(new { message = "Login Successfully", accessToken });
        }

        //[Authorize(Roles = $"{nameof(UserRole.Customer)}, {nameof(UserRole.Artist)}"), HttpPost("switch-profile")]
        //public async Task<IActionResult> SwitchProfile()
        //{
        //    var authenticatedResponseModel = await authenticationBLL.SwitchProfile();
        //    return Ok(new { message = "Switch Profile Successfully", authenticatedResponseModel });
        //}

        [AllowAnonymous, HttpPost("email-verification-resend")]
        public async Task<IActionResult> ResendEmailConfirm()
        {
            await authenticationBLL.ReActiveAccountByToken();

            return Ok(new { message = "Email has sent to user's mail" });
        }

        [AllowAnonymous, HttpPost("google-login")]
        public async Task<IActionResult> LoginByGoogle([FromBody] GoogleLoginRequestModel googleToken)
        {
            var token = await authenticationBLL.LoginByGoogle(googleToken.GoogleToken);
            return Ok(new { token });
        }

        [AllowAnonymous, HttpPost("google-account-confirmation")]
        public async Task<IActionResult> ConfirmLinkWithGoogleAccount([FromBody] string email)
        {
            var authenticatedResponseModel = await authenticationBLL.ConfirmLinkWithGoogleAccount(email);
            return Ok(new { message = "Login Successfully", authenticatedResponseModel });
            //return Redirect(returnUrl); // Tự động chuyển hướng đến returnURL không cần phải nhờ FE chuyển FE chỉ cần bỏ URL vào biến returnURL
        }


        /// <summary>
        /// Quên mật khẩu, điền email đăng ký tài khoản và hàm gửi OTP qua email vừa nhập
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            await authenticationBLL.SendOTPForgotPasswordAsync(model);
            return Ok();
        }

        /// <summary>
        /// Tạo OTP mới và gửi OTP qua email (cho việc forgot-password ---> reset password)
        /// </summary>
        /// <param name="email">email nhận được OTP</param>
        /// <param name="otpCode">OTP xác thực từ email trên</param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("otp-confirmation")]
        public async Task<IActionResult> ValidateOTP([FromBody] string otpCode, string email) //chỗ này đang ?, ko biết Hòa lấy OTP với cái gì nên đang để tạm
        {
            await authenticationBLL.ValidateOTPPassword(email, otpCode);
            return Ok(new { message = "Reset password successfully, please check your email to get new password." });
        }


        /// <summary>
        /// Đặt lại mật khẩu 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            await authenticationBLL.ResetPasswordAsync(model);
            return Ok(new { message = "Reset password successfully" });
        }


        /// <summary>
        /// Lấy thông tin đăng nhập của người dùng từ jwt ở header
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Customer)}, {nameof(UserRole.Artist)}, {nameof(UserRole.Admin)}, {nameof(UserRole.ContentManager)}"), HttpGet("authenticated-user-info")]
        public IActionResult GetAuthenticatedUserInfo()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var authenticatedUserInfoResponseModel = authenticationBLL.GetUserInformation(token);
            return Ok(new { authenticatedUserInfoResponseModel });
        }


        /// <summary>
        /// Cấp lại access và refresh token mới khi access token cũ hết hạn
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("refresh-token")]
        public IActionResult Relog()
        {
            var authenticatedUserInfoResponseModel = authenticationBLL.Relog();
            return Ok(new { authenticatedUserInfoResponseModel });
        }


        /// <summary>
        /// Đăng xuất, cần FE xóa accessToken ra khỏi localStorage
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = $"{nameof(UserRole.Customer)}, {nameof(UserRole.Artist)}, {nameof(UserRole.Admin)}, {nameof(UserRole.ContentManager)}"), HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await authenticationBLL.LogoutAsync();
            return Ok(new { message = "Logout Successfully" });
        }

    }
}

