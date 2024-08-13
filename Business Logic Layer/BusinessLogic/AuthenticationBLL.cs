using AutoMapper;
using Business_Logic_Layer.Models;
using Business_Logic_Layer.Services.EmailSender;
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
        private readonly IEmailSenderCustom _emailSender;
        private readonly ILogger<CustomerBLL> _logger;

        public AuthenticationBLL(ILogger<CustomerBLL> logger, IMapper mapper, IAuthenticationRepository authenticationRepository, IEmailSenderCustom emailSender)
        {
            _logger = logger;
            _mapper = mapper;
            _authenticationRepository = authenticationRepository;
            _emailSender = emailSender;
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

                //string token = GenerateConfirmationToken(user); // Tạo token xác nhận
                // Sau khi tạo xong thì mã hóa nó nếu chưa mã hóa sau đó tạo link như dưới
                // Dùng mã hóa cho email khi tạo link
                //string confirmationLink = $"https://myfrontend.com/confirm-email?email={HCMA(email)}&token={HCMA(token)}";
                //user.Token = token;

                await _authenticationRepository.CreateAccount(user);

                await _emailSender.SendEmailConfirmationAsync(user, "Xác nhận Email", "https://www.google.com/logos/doodles/2024/paris-games-artistic-swimming-6753651837110445-la202124.gif");

                // Confirmation Link nên redirect tới đường dẫn trang web bên FE sau đó khi tới đó thì FE sẽ gọi API bên BE để xác nhận đăng ký
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

        public async Task ActivateAccountByToken(string email, string token)
        {
            // Giải mã token vừa nhận được
            // string verifiedToken = Decode(token);
            // Sau đó kiểm tra token nhận được so với token từ db

            var retrieveToken = await _authenticationRepository.GetAccountToken(email);

            if(retrieveToken != token) // Đổi token bằng verifiedToken
            {
                throw new ArgumentException("Token and Retrieve Token does not matches", "ativateAccountFail");
            }

            await _authenticationRepository.UpdateAccountConfirmationStatus(email);

            return;
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
    }
}
