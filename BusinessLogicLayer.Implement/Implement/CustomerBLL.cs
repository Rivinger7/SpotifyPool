using AutoMapper;
using Business_Logic_Layer.Models;
using Data_Access_Layer.Entities;
using Data_Access_Layer.Implement.Repositories.Single.Accounts.Customers;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.Implement.Implement
{
    public class CustomerBLL
    {
        private readonly IMapper _mapper;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerBLL> _logger;

        public CustomerBLL(ILogger<CustomerBLL> logger, IMapper mapper, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerModel>> GetAllUsers()
        {
            var users = await _customerRepository.GetAllUsers();
            IEnumerable<CustomerModel> customerModel = _mapper.Map<IEnumerable<User>, IEnumerable<CustomerModel>>(users);
            return customerModel;
        }
    }
}
