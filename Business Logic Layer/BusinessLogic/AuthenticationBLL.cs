using AutoMapper;
using Business_Logic_Layer.Models;
using Business_Logic_Layer.Services.EmailSender;
using Business_Logic_Layer.Services.JWT;
using Data_Access_Layer.Repositories;
using Data_Access_Layer.Repositories.Accounts.Authentication;
using Data_Access_Layer.Repositories.Accounts.Customers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.BusinessLogic
{
    public class AuthenticationBLL
    {
        private readonly IMapper _mapper;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailSenderCustom _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerBLL> _logger;

        public AuthenticationBLL(ILogger<CustomerBLL> logger, IMapper mapper, IAuthenticationRepository authenticationRepository, ICustomerRepository customerRepository, IEmailSenderCustom emailSender, IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _authenticationRepository = authenticationRepository;
            _customerRepository = customerRepository;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task CreateAccount(RegisterModel registerModel)
        {
            try
            {
                string password = registerModel.Password;
                string email = registerModel.Email;

                string passwordHashed = BCrypt.Net.BCrypt.HashPassword(password);
                registerModel.Password = passwordHashed;

                User user = _mapper.Map<RegisterModel, User>(registerModel);

                await _authenticationRepository.CheckAccountExists(user);

                // Sau khi tạo xong thì mã hóa nó nếu chưa mã hóa sau đó tạo link như dưới
                // Dùng mã hóa cho email khi tạo link
                string encryptedToken = Shared.Helpers.DataEncryptionExtensions.HmacSHA256(email, _configuration.GetSection("JWTSettings:SecretKey").Value);
                JWT jwtGenerator = new JWT(_configuration);
                string token = jwtGenerator.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
                string confirmationLink = $"https://myfrontend.com/confirm-email?token={token}";
                user.Token = encryptedToken;
                
                await _authenticationRepository.CreateAccount(user);

                await _emailSender.SendEmailConfirmationAsync(user, "Xác nhận Email", confirmationLink);

                // Confirmation Link nên redirect tới đường dẫn trang web bên FE sau đó khi tới đó thì FE sẽ gọi API bên BE để xác nhận đăng ký
            }
            catch (ArgumentException aex)
            {
                throw new ArgumentException(aex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task ActivateAccountByToken(string token)
        {
            try
            {
                var validator = new JWT(_configuration);
                var decodedToken = validator.DecodeToken(token);

                var emailClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == "Email");
                //var roleClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
                var encryptedTokenClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == "EncrpytedToken");

                if (emailClaim == null || encryptedTokenClaim == null)
                {
                    throw new ArgumentException("Token is missing required claims", "activateAccountFails");
                }

                var email = emailClaim.Value;
                //var role = roleClaim.Value;
                var encryptedToken = encryptedTokenClaim.Value;

                //_logger.LogInformation("Token decoded successfully");
                //_logger.LogInformation($"Email: {email} || Role: , || EncryptedToken: {encryptedToken}");

                var retrieveToken = await _authenticationRepository.GetAccountToken(email);

                if (retrieveToken != encryptedToken) // Đổi token bằng verifiedToken
                {
                    throw new ArgumentException("Token and Retrieve Token does not matches", "ativateAccountFail");
                }

                await _authenticationRepository.UpdateAccountConfirmationStatus(email);

                return;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<CustomerModel> Authenticate(LoginModel loginModel)
        {
            var passwordHashed = await GetPasswordHashed(loginModel.Username);

            bool isPasswordHashed = BCrypt.Net.BCrypt.Verify(loginModel.Password, passwordHashed);
            if (!isPasswordHashed)
            {
                throw new ArgumentException("Username or Password is incorrect", "loginFail");
            }
           
            loginModel.Password = passwordHashed;

            User user = _mapper.Map<LoginModel, User>(loginModel);

            var retrievedUser = await _authenticationRepository.Authenticate(user);

            CustomerModel customerModel = _mapper.Map<User, CustomerModel>(retrievedUser);

            return customerModel;
        }

        private async Task<string> GetPasswordHashed(string username)
        {
            try
            {
                var passwordHashed = await _authenticationRepository.GetPasswordHashed(username);
                return passwordHashed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task ReActiveAccountByToken(string username)
        {
            try
            {
                var user = await _customerRepository.GetByUsername(username);

                string email = user.Email;

                string encryptedToken = Shared.Helpers.DataEncryptionExtensions.HmacSHA256(email, _configuration.GetSection("JWTSettings:SecretKey").Value);
                JWT jwtGenerator = new JWT(_configuration);
                string token = jwtGenerator.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
                string confirmationLink = $"https://myfrontend.com/confirm-email?token={token}";
                await _authenticationRepository.UpdateTokenByEmail(email, encryptedToken);

                await _emailSender.SendEmailConfirmationAsync(user, "Xác nhận Email", confirmationLink);

                return;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
