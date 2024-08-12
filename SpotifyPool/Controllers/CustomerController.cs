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

        public CustomerController(CustomerBLL customerBLL)
        {
            _customerBLL = customerBLL;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _customerBLL.GetAllUsers();
            return users is not null ? Ok(users) : NotFound("Not Found!!!");
        }
    }
}
