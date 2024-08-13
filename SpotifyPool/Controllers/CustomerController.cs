using Business_Logic_Layer.BusinessLogic;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerBLL _customerBLL;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerBLL customerBLL, ILogger<CustomerController> logger)
        {
            _customerBLL = customerBLL;
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
