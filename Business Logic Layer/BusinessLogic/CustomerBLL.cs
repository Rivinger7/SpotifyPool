﻿using AutoMapper;
using Business_Logic_Layer.Models;
using Data_Access_Layer.Repositories;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.BusinessLogic
{
    public class CustomerBLL
    {
        private readonly IMapper _mapper;
        private ICustomerRepository _customerRepository;
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
