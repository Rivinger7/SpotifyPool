using AutoMapper;
using BusinessLogicLayer.Enum.Services.User;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.EmailSender;
using BusinessLogicLayer.Interface.Microservices_Interface.Geolocation;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView;
using BusinessLogicLayer.ModelView.Microservice_Model_Views.Geolocation.Response;
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
    public class AuthenticationBLL(ILogger<AuthenticationBLL> logger, IMapper mapper, SpotifyPoolDBContext context, IJwtBLL jwtBLL, IEmailSenderCustom emailSender, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IGeolocation geolocation) : IAuthenticationBLL
    {
        private readonly IMapper _mapper = mapper;
        private readonly SpotifyPoolDBContext _context = context;
        private readonly IJwtBLL _jwtBLL = jwtBLL;
        private readonly IEmailSenderCustom _emailSender = emailSender;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthenticationBLL> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IGeolocation _geolocation;

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

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            //bool isExistingUser = await _context.Users.Find(user => user.UserName == username).AnyAsync();
            //if (isExistingUser)
            //{
            //    throw new DataExistCustomException("UserName already exists");
            //}

            await CheckAccountExists(registerModel);

            // Sau khi tạo xong thì mã hóa nó nếu chưa mã hóa sau đó tạo link như dưới
            // Dùng mã hóa cho email khi tạo link
            string encryptedToken = DataEncryptionExtensions.HmacSHA256(email, Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found"));
            string token = _jwtBLL.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
            string confirmationLink = $"https://myfrontend.com/confirm-email?token={token}";

            User newUser = new()
            {
                UserName = username,
                Password = passwordHash,
                Email = email,
                PhoneNumber = registerModel.PhoneNumber,
                Role = "Customer",
                Status = UserStatus.Inactive,
                TokenEmailConfirm = encryptedToken
            };

            await _context.Users.InsertOneAsync(newUser);
            //var result = await _userManager.CreateAsync(newUser, password);

            //if (!result.Succeeded)
            //{
            //    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            //    throw new BadRequestCustomException($"Cannot handle password: {errors}");
            //}

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
            UpdateDefinition<User> update = Builders<User>.Update.Set(user => user.Status, UserStatus.Active).Set(user => user.TokenEmailConfirm, null);

            UpdateResult result = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, update);

            if (result.ModifiedCount < 1)
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

            //var claimss = result.Principal.Identities
            //    .FirstOrDefault()?.Claims
            //    .Select(claim => new
            //    {
            //        claim.Type,
            //        claim.Value
            //    });
            //foreach(var claim in claimss)
            //{
            //    Console.WriteLine("===========");
            //    Console.WriteLine(claim);
            //    Console.WriteLine("===========");
            //}

            IEnumerable<Claim>? claims = result.Principal.Identities.FirstOrDefault()?.Claims.ToList();

            string? givenName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value; // Given Name can be null
            string? surName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value; // Sur Name can be null
            string? fullName = Util.ValidateAndCombineName(givenName, surName);

            // Lấy URL ảnh người dùng
            string avatar = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value ?? throw new ArgumentNullCustomException(nameof(avatar), "Avatar is null");

            // Lấy Email người dùng
            string email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? throw new ArgumentNullCustomException(nameof(email), "Email is null");

            // Lấy thông tin người dùng
            User retrieveUser = await _context.Users.Find(user => user.Email == email).FirstOrDefaultAsync();

            // Lấy thông tin ảnh từ URL
            // var = System.Drawing.Image
            var imageInfo = await Util.GetImageInfoFromUrl(avatar) ?? throw new InternalServerErrorCustomException("Unable to retrieve image information from the provided URL.");
            int? imageHeight = imageInfo.Height;
            int? imageWidth = imageInfo.Width;

            // Lấy thông tin IP Address
            // Dùng SDK đi!!!
            //GeolocationResponseModel geolocationResponseModel = await _geolocation.GetLocationAsync();

            // Trường hợp mới đăng nhập google account lần đầu tức là chưa tồn tại tài khoản trong db
            if (retrieveUser is null)
            {
                retrieveUser = new User()
                {
                    FullName = fullName,
                    Email = email,
                    Images =
                    [
                        new()
                        {
                            URL = avatar,
                            Height = imageHeight ?? 96,
                            Width = imageWidth ?? 96,
                        },
                    ],
                    Role = "Customer",
                    Product = UserProduct.Free,
                    //CountryId = geolocationResponseModel.CountryCode2,
                    Status = UserStatus.Active,
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

            User retrieveUser = await _context.Users.Find(user => user.UserName == username).FirstOrDefaultAsync() ?? throw new ArgumentException("Username or Password is incorrect");

            switch (retrieveUser.Status)
            {
                case UserStatus.Inactive: throw new UnAuthorizedCustomException("Not active");
                case UserStatus.Banned: throw new UnAuthorizedCustomException("Banned");
            }

            bool isPasswordHashed = BCrypt.Net.BCrypt.Verify(loginModel.Password, retrieveUser.Password);
            if (!isPasswordHashed)
            {
                throw new ArgumentException("Username or Password is incorrect");
            }

            // JWT
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.Name, retrieveUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, retrieveUser.Role)
                ];

            //Call method to generate access token
            _jwtBLL.GenerateAccessToken(claims, retrieveUser, out string accessToken, out string refreshToken);

            //if (accessToken is null)
            //{
            //    await Console.Out.WriteLineAsync("accesstoken is null");
            //}

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

            User retrieveUser = await _context.Users.Find(user => user.UserName == username).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("Not found any user");

            string email = retrieveUser.Email;

            string encryptedToken = DataEncryptionExtensions.HmacSHA256(email, _configuration.GetSection("JWTSettings:SecretKey").Value);

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

        public Task<string> SendTokenForgotPasswordAsync(ForgotPasswordRequestModel model)
        {
            throw new NotImplementedException();
        }

        public Task ResetPasswordAsync(ResetPasswordRequestModel model)
        {
            throw new NotImplementedException();
        }

        //public async Task<string> SendTokenForgotPasswordAsync(ForgotPasswordRequestModel model)
        //{
        //    // Lấy thông tin người dùng
        //    User retrieveUser = await _userManager.FindByEmailAsync(model.Email)
        //                                        ?? throw new DataNotFoundCustomException("There's no account match with this email!");

        //    // Tạo token cho forgot password
        //    string? token = await _userManager.GeneratePasswordResetTokenAsync(retrieveUser);

        //    Dictionary<string, string?> parameter = new()
        //    {
        //        {"token", token },
        //        {"email", model.Email }
        //    };

        //    // tạo chuỗi url có 2 tham số trên 
        //    string? callback = QueryHelpers.AddQueryString(model.ClientUrl, parameter);

        //    // tạo message để gửi email
        //    Message message = new Message([retrieveUser.Email], "Reset Password", callback);

        //    await _emailSender.SendEmailForgotPasswordAsync(retrieveUser, message);


        //    return token;
        //}

        //public async Task ResetPasswordAsync(ResetPasswordRequestModel model)
        //{
        //    // tìm user theo email
        //    User user = await _userManager.FindByEmailAsync(model.Email) ?? throw new DataNotFoundCustomException("Email does not match with any account!");



        //    if (model.NewPassword != model.ConfirmPassword)
        //    {
        //        throw new BadRequestCustomException("New password and confirm password do not match!");
        //    }

        //    // reset password
        //    IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        throw new BadRequestCustomException("Reset password failed!");
        //    }
        //}
    }
}
