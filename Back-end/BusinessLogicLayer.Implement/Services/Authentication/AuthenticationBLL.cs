using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.EmailSender;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Authentication
{
    public class AuthenticationBLL : IAuthenticationBLL
    {
        private readonly IMapper _mapper;
        private readonly SpotifyPoolDBContext _context;
        private readonly IJwtBLL _jwtBLL;
        private readonly IEmailSenderCustom _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationBLL> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthenticationBLL(ILogger<AuthenticationBLL> logger, IMapper mapper, SpotifyPoolDBContext context, IJwtBLL jwtBLL, IEmailSenderCustom emailSender, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _jwtBLL = jwtBLL;
            _emailSender = emailSender;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task CreateAccount(RegisterRequestModel registerModel)
        {
            string username = registerModel.UserName;
            string password = registerModel.Password;
            string email = registerModel.Email;
            string confirmedPassword = registerModel.ConfirmedPassword;

            bool isConfirmedPassword = password == confirmedPassword;
            if (!isConfirmedPassword)
            {
                throw new InvalidDataCustomException("Password and Confirmed Password do not match");
            }

            //string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            bool isExistingUser = await _context.Users.Find(user => user.UserName == username).AnyAsync();
            if (isExistingUser)
            {
                throw new DataExistCustomException("UserName already exists");
            }

            await CheckAccountExists(registerModel);

            // Sau khi tạo xong thì mã hóa nó nếu chưa mã hóa sau đó tạo link như dưới
            // Dùng mã hóa cho email khi tạo link
            string encryptedToken = DataEncryptionExtensions.HmacSHA256(email, Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found"));
            string token = _jwtBLL.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
            string confirmationLink = $"https://myfrontend.com/confirm-email?token={token}";

            User newUser = new()
            {
                UserName = username,
                //Password = passwordHash,
                Email = email,
                PhoneNumber = registerModel.PhoneNumber,
                Role = "Customer",
                Status = "Inactive",
                TokenEmailConfirm = encryptedToken
            };

            //await _context.Users.InsertOneAsync(newUser);
            var result = await _userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestCustomException($"Cannot handle password: {errors}");
            }

            await _emailSender.SendEmailConfirmationAsync(newUser, "Xác nhận Email", confirmationLink);

            // Confirmation Link nên redirect tới đường dẫn trang web bên FE sau đó khi tới đó thì FE sẽ gọi API bên BE để xác nhận đăng ký

        }

        private async Task CheckAccountExists(RegisterRequestModel registerModel)
        {
            string username = registerModel.UserName;
            string email = registerModel.Email;
            string phonenumber = registerModel.PhoneNumber;

            // Tạo bộ lọc kết hợp các điều kiện (UserName, Email, PhoneNumber)
            FilterDefinition<User> filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(user => user.UserName, username),
                Builders<User>.Filter.Eq(user => user.Email, email),
                Builders<User>.Filter.Eq(user => user.PhoneNumber, phonenumber)
            );

            // Thực hiện truy vấn tới MongoDB để tìm xem có tài khoản nào khớp không
            IEnumerable<User> existingUsers = await _context.Users.Find(filter).ToListAsync();

            // Kiểm tra từng điều kiện và ném lỗi phù hợp
            if (existingUsers.Any(user => user.UserName == username))
            {
                throw new DataExistCustomException("UserName already exists");
            }

            if (existingUsers.Any(user => user.Email == email))
            {
                throw new DataExistCustomException("Email already exists");
            }

            if (existingUsers.Any(user => user.PhoneNumber == phonenumber))
            {
                throw new DataExistCustomException("Phone number already exists");
            }
        }

        public async Task ActivateAccountByToken(string token)
        {
            JwtSecurityToken decodedToken = _jwtBLL.DecodeToken(token);

            Claim? emailClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == "Email");
            //var roleClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
            Claim? encryptedTokenClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == "EncrpytedToken");

            if (emailClaim == null || encryptedTokenClaim == null)
            {
                throw new DataNotFoundCustomException("TokenEmailConfirm is missing required claims");
            }

            string email = emailClaim.Value;
            //var role = roleClaim.Value;
            string encryptedToken = encryptedTokenClaim.Value;

            //_logger.LogInformation("TokenEmailConfirm decoded successfully");
            //_logger.LogInformation($"Email: {email} || Role: , || EncryptedToken: {encryptedToken}");

            User retrieveUser = await _context.Users.Find(user => user.Email == email).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("Not found any user");
            if (retrieveUser.TokenEmailConfirm != encryptedToken)
            {
                throw new InvalidDataCustomException("TokenEmailConfirm and Retrieve TokenEmailConfirm do not match");
            }

            await UpdateAccountConfirmationStatus(retrieveUser);

            return;
        }

        private async Task UpdateAccountConfirmationStatus(User retrieveUser)
        {
            UpdateDefinition<User> statusUpdate = Builders<User>.Update.Set(user => user.Status, "Active");

            UpdateDefinition<User> tokenUpdate = Builders<User>.Update.Set(user => user.TokenEmailConfirm, null);

            UpdateResult statusResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, statusUpdate);

            UpdateResult tokenResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, tokenUpdate);

            // Kiểm tra nếu có ít nhất một field_name được cập nhật
            if (statusResult.ModifiedCount < 1 || tokenResult.ModifiedCount < 1)
            {
                throw new CustomException("Update Fail", StatusCodes.Status500InternalServerError, "Found user but can not update");
            }

            return;
        }

        public async Task<AuthenticatedResponseModel> LoginByGoogle()
        {
            AuthenticateResult result = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal == null)
            {
                throw new ArgumentNullCustomException(nameof(result.Principal), "Principal is null");
            }

            //var claims = result.Principal.Identities
            //	.FirstOrDefault()?.Claims
            //	.Select(claim => new
            //	{
            //		claim.Type,
            //		claim.Value
            //	});

            IEnumerable<Claim>? claims = result.Principal.Identities.FirstOrDefault()?.Claims.ToList();
            string? givenName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value; // Given Name can be null
            string? surName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value; // Sur Name can be null
            string? fullName = Util.ValidateAndCombineName(givenName, surName);
            string email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? throw new ArgumentNullCustomException(nameof(email), "Email is null");

            // Lấy thông tin người dùng
            User retrieveUser = await _context.Users.Find(user => user.Email == email).FirstOrDefaultAsync();

            // Trường hợp mới đăng nhập google account lần đầu tức là chưa tồn tại tài khoản trong db
            if (retrieveUser is null)
            {
                retrieveUser = new User()
                {
                    FullName = fullName,
                    Email = email,
                    Role = "Customer",
                    Status = "Active",
                };
                await _context.Users.InsertOneAsync(retrieveUser);
            }
            else // Kiểm tra user có liên kết google account chưa
            {
                // Nếu chưa liên kết thì yêu cầu người dùng muốn liên kết hay không
                bool? isLinkedWithGoogle = retrieveUser.IsLinkedWithGoogle;
                if (isLinkedWithGoogle is not null && !isLinkedWithGoogle.Value) // isLink = False
                {
                    // Sau khi gọi API thành công thì bên FE sẽ kiểm tra nếu access token là rỗng hoặc null thì tức là cần xác nhận liên kết tài khoản Google
                    string confirmationLink = $"https://myfrontend.com/confirm-link-with-google-account";

                    // Hoặc có thể sử dụng json nullable tức là thêm field của AuthResponse model view
                    // Nếu null thì json sẽ không hiển thị ra field đó

                    return new AuthenticatedResponseModel { ConfirmationLinkWithGoogleAccount = confirmationLink };
                }
            }

            // Nếu không cho phép liên kết thì không cho người dùng đăng nhập bằng google account vì account đã tồn tại google email rồi
            // Nếu cho phép liên kết confirm liên kết
            // Tạo API để FE gọi
            // Cái này bên FE sẽ tự xử lý

            // Nếu có liên kết thì đăng nhập bình thường
            // Trường hợp đã đăng nhập vào hệ thống trước đó rồi
            // Tạo JWT access token và refresh token
            JWTGenerator(retrieveUser, out string accessToken, out string refreshToken);

            // New object ModelView
            AuthenticatedResponseModel authenticationModel = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return authenticationModel;
        }

        public async Task<AuthenticatedResponseModel> ConfirmLinkWithGoogleAccount(string email)
        {
            // Lấy thông tin người dùng
            User retrieveUser = await _context.Users.Find(user => user.Email == email && user.IsLinkedWithGoogle == false).FirstOrDefaultAsync() ?? throw new CustomException("Google Account Linking", 44, "Not found user or user has been linked with google account");

            // Cập nhật trạng thái liên kết tài khoản Google
            UpdateDefinition<User> isLinkedWithGoogleUpdate = Builders<User>.Update.Set(user => user.IsLinkedWithGoogle, true);
            UpdateResult isLinkedWithGoogleUpdateResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, isLinkedWithGoogleUpdate);

            // Kiểm tra nếu có ít nhất một field_name được cập nhật
            if (isLinkedWithGoogleUpdateResult.ModifiedCount < 1)
            {
                throw new CustomException("Update Fail", StatusCodes.Status500InternalServerError, "Found user but can not update");
            }

            // Tạo JWT access token và refresh token
            JWTGenerator(retrieveUser, out string accessToken, out string refreshToken);

            // New object ModelView
            AuthenticatedResponseModel authenticationModel = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return authenticationModel;
        }

        /// <summary>
        /// Phương thức này dùng để tạo ra JWT access token và refresh token dựa trên các claim của người dùng.
        /// </summary>
        /// <param name="retrieveUser">Đối tượng người dùng đã được lấy từ cơ sở dữ liệu hoặc hệ thống.</param>
        /// <param name="accessToken">Biến out dùng để lưu JWT access token được tạo ra.</param>
        /// <param name="refreshToken">Biến out dùng để lưu refresh token được tạo ra.</param>
        private void JWTGenerator(User retrieveUser, out string accessToken, out string refreshToken)
        {
            // Có thể không cần dùng claimList vì trên đó đã có list về claim và tùy theo hệ thống nên tạo mới list claim
            IEnumerable<Claim> claimsList =
                [
                    new Claim(ClaimTypes.Name, retrieveUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, retrieveUser.Role)
                ];

            // Gọi phương thức để tạo access token và refresh token từ danh sách claim và thông tin người dùng
            _jwtBLL.GenerateAccessToken(claimsList, retrieveUser, out accessToken, out refreshToken);

            return;
        }

        public async Task<AuthenticatedResponseModel> Authenticate(LoginRequestModel loginModel)
        {
            string username = loginModel.Username;
            string password = loginModel.Password;

            // get user by UserManager
            User retrieveUser = await _userManager.FindByNameAsync(username)
                                          ?? throw new DataNotFoundCustomException("UserName or Password is incorrect");



            switch (retrieveUser.Status.ToLower())
            {
                case "inactive": throw new UnAuthorizedCustomException("Not active");
                case "banned": throw new UnAuthorizedCustomException("Banned");
            }


            SignInResult result = await _signInManager.PasswordSignInAsync(retrieveUser, password, false, false);


            if (!result.Succeeded)
            {
                throw new BadRequestCustomException("username or password is not correct!");
            }

            // JWT
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.Name, retrieveUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, retrieveUser.Role)
                ];

            //Call method to generate access token
            _jwtBLL.GenerateAccessToken(claims, retrieveUser, out string accessToken, out string refreshToken);

            if (accessToken is null)
            {
                await Console.Out.WriteLineAsync("accesstoken is null");
            }

            // New object ModelView
            AuthenticatedResponseModel authenticationModel = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return authenticationModel;

        }

        public async Task ReActiveAccountByToken(string username)
        {

            User retrieveUser = await _userManager.FindByNameAsync(username) ?? throw new DataNotFoundCustomException("Not found any user");

            string email = retrieveUser.Email;

            string encryptedToken = DataEncryptionExtensions.HmacSHA256(email, Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found"));

            string token = _jwtBLL.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
            string confirmationLink = $"https://myfrontend.com/confirm-email?token={token}";

            UpdateDefinition<User> tokenUpdate = Builders<User>.Update.Set(user => user.TokenEmailConfirm, token);
            UpdateResult tokenResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, tokenUpdate);

            if (tokenResult.ModifiedCount < 1)
            {
                throw new CustomException("Update Fail", StatusCodes.Status500InternalServerError, "Found user but can not update");
            }

            await _emailSender.SendEmailConfirmationAsync(retrieveUser, "Xác nhận Email", confirmationLink);

            return;
        }

        public async Task<string> SendTokenForgotPasswordAsync(ForgotPasswordRequestModel model)
        {
            // Lấy thông tin người dùng
            User retrieveUser = await _userManager.FindByEmailAsync(model.Email)
                                                ?? throw new DataNotFoundCustomException("There's no account match with this email!");

            // Tạo token cho forgot password
            string? token = await _userManager.GeneratePasswordResetTokenAsync(retrieveUser);

            Dictionary<string, string?> parameter = new()
            {
                {"token", token },
                {"email", model.Email }
            };

            // tạo chuỗi url có 2 tham số trên 
            string? callback = QueryHelpers.AddQueryString(model.ClientUrl, parameter);

            // tạo message để gửi email
            Message message = new Message([retrieveUser.Email], "Reset Password", callback);

            await _emailSender.SendEmailForgotPasswordAsync(retrieveUser, message);


            return token;
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestModel model)
        {
            // tìm user theo email
            User user = await _userManager.FindByEmailAsync(model.Email) ?? throw new DataNotFoundCustomException("Email does not match with any account!");



            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new BadRequestCustomException("New password and confirm password do not match!");
            }

            // reset password
            IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                throw new BadRequestCustomException("Reset password failed!");
            }
        }
    }
}
