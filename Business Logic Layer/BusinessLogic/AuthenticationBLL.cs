using AutoMapper;
using Business_Logic_Layer.Models;
using Data_Access_Layer.Repositories;
using Data_Access_Layer.Repositories.Accounts.Authentication;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.BusinessLogic
{
    public class AuthenticationBLL
    {
        private readonly IMapper _mapper;
        private IAuthenticationRepository _authenticationRepository;
        private readonly ILogger<CustomerBLL> _logger;

        public AuthenticationBLL(ILogger<CustomerBLL> logger, IMapper mapper, IAuthenticationRepository authenticationRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _authenticationRepository = authenticationRepository;
        }

        public async Task CreateAccount(RegisterModel registerModel)
        {
            try
            {
                User user = _mapper.Map<RegisterModel, User>(registerModel);

                await _authenticationRepository.CheckAccountExists(user);
                await _authenticationRepository.CreateAccount(user);
            }
            catch(ArgumentException aex)
            {
                throw new ArgumentException(aex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<CustomerModel> Authenticate(LoginModel loginModel)
        {
            User user = _mapper.Map<LoginModel, User>(loginModel);

            var retrievedUser = await _authenticationRepository.Authenticate(user);

            CustomerModel customerModel = _mapper.Map<User, CustomerModel>(retrievedUser);

            return customerModel;
        }
    }
}
