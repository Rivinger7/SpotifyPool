using Business_Logic_Layer.BusinessLogic;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpotifyPool.JIRA_REST_API.Issues;

namespace SpotifyPool.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerBLL _customerBLL;
        private readonly IssueClient _issueClient;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerBLL customerBLL, IssueClient issueClient, ILogger<CustomerController> logger)
        {
            _customerBLL = customerBLL;
            _issueClient = issueClient;
            _logger = logger;

        }

        
        [HttpGet("GetAllUsers")]
		public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _customerBLL.GetAllUsers();
                return users is not null ? Ok(users) : NotFound("Not Found!!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = "Internal server error", stackerror = ex.StackTrace});
            }
        }
    }
}
