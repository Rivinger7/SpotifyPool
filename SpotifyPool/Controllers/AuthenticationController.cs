using BCrypt.Net;
using Business_Logic_Layer.BusinessLogic;
using Business_Logic_Layer.Models;
using Data_Access_Layer.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace SpotifyPool.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationBLL _authenticationBLL;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(AuthenticationBLL authenticationBLL, ILogger<AuthenticationController> logger)
        {
            _authenticationBLL = authenticationBLL;
            _logger = logger;
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
            catch(ArgumentException aex)
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
                return Ok(new { message = "Login Successfully", customerModel });
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
    }
}
