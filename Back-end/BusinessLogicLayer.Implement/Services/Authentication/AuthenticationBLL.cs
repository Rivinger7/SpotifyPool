﻿using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.EmailService;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView;
using BusinessLogicLayer.ModelView.Microservice_Model_Views.EmailService;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.EmailSender.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SetupLayer.Enum.Services.User;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Utility.Coding;
using Utility.EmailTemplate;

namespace BusinessLogicLayer.Implement.Services.Authentication
{
    public class AuthenticationBLL(IConnectionMultiplexer connectionMultiplexer, IMapper mapper, IUnitOfWork unitOfWork, IJwtBLL jwtBLL, IHttpContextAccessor httpContextAccessor, IEmailService emailService) : IAuthentication
    {
        private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJwtBLL _jwtBLL = jwtBLL;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IEmailService _emailService = emailService;

        public async Task CreateAccount(RegisterRequestModel registerModel)
        {
            string username = registerModel.UserName;
            string password = registerModel.Password;
            string displayName = registerModel.DisplayName;
            string email = registerModel.Email;
            string confirmedPassword = registerModel.ConfirmedPassword;

            bool isConfirmedPassword = password == confirmedPassword;
            if (!isConfirmedPassword)
            {
                throw new InvalidDataCustomException("Password and Confirmed Password do not match");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await CheckAccountExists(registerModel);

            // Sau khi tạo xong thì mã hóa nó nếu chưa mã hóa sau đó tạo link như dưới
            // Dùng mã hóa cho email khi tạo link
            string encryptedToken = DataEncryptionExtensions.HmacSHA256(email, Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Mode property is not set in environment or not found"));
            string token = _jwtBLL.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
            string confirmationLink = $"http://localhost:5173/spotifypool/confirm-email?token={token}";

            // Avatar mặc định
            string avatarUrl = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730097883/60d5dc467b950c5ccc8ced95_spotify-for-artists_on4me9.jpg";

            User newUser = new()
            {
                DisplayName = displayName,
                UserName = username,
                Password = passwordHash,
                Email = email,
                PhoneNumber = registerModel.PhoneNumber,
                Gender = UserGender.NotSpecified,
                Roles = [UserRole.Customer],
                Product = UserProduct.Free,
                IsLinkedWithGoogle = false,
                CountryId = "VN",
                Status = UserStatus.Inactive,
                CreatedTime = Util.GetUtcPlus7Time(),
                TokenEmailConfirm = encryptedToken,
                Images =
                [
                    new()
                    {
                        URL = avatarUrl,
                        Height = 600,
                        Width = 600,
                    },
                    new()
                    {
                        URL = avatarUrl,
                        Height = 300,
                        Width = 300,
                    },
                    new()
                    {
                        URL = avatarUrl,
                        Height = 64,
                        Width = 64,
                    }
                ]
            };

            await _unitOfWork.GetCollection<User>().InsertOneAsync(newUser);

            // Thêm projection giúp mình lấy thông tin cần thiết từ User
            UserResponseModel userResponseModel = _mapper.Map<UserResponseModel>(newUser);

            // Tạo request model để gửi email
            EmailMetadata emailMetadata = new(newUser.Email, "Email confirmation", confirmationLink);

            // Gửi email
            await _emailService.SendAsync(emailMetadata, 1);

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
            IEnumerable<User> existingUsers = await _unitOfWork.GetCollection<User>().Find(filter).ToListAsync();

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
            Claim? encryptedTokenClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == "EncrpytedToken");

            if (emailClaim == null || encryptedTokenClaim == null)
            {
                throw new DataNotFoundCustomException("TokenEmailConfirm is missing required claims");
            }

            string email = emailClaim.Value;
            string encryptedToken = encryptedTokenClaim.Value;

            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.Email == email).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("Not found any user");
            if (retrieveUser.TokenEmailConfirm != encryptedToken)
            {
                throw new InvalidDataCustomException("TokenEmailConfirm and Retrieve TokenEmailConfirm do not match");
            }

            await UpdateAccountConfirmationStatus(retrieveUser);

            // Xóa session khi người dùng resend email confirm
            // Session này phục vụ cho hàm ReActiveAccountByToken khi chưa active account
            _httpContextAccessor.HttpContext?.Session.Remove("UserNameTemp");

            return;
        }

        private async Task UpdateAccountConfirmationStatus(User retrieveUser)
        {
            UpdateDefinition<User> update = Builders<User>.Update.Set(user => user.Status, UserStatus.Active).Set(user => user.TokenEmailConfirm, null);

            UpdateResult result = await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == retrieveUser.Id, update);

            if (result.ModifiedCount < 1)
            {
                throw new CustomException("Update Fail", StatusCodes.Status500InternalServerError, "Found user but can not update");
            }

            return;
        }

        public async Task<AuthenticatedResponseModel> LoginByGoogle(string googleToken)
        {
            if (string.IsNullOrEmpty(googleToken))
            {
                throw new CustomException("Google Token", StatusCodes.Status500InternalServerError, "Google Token is null or empty");
            }

            // Lấy token từ FE
            // Xác thực token của Google và lấy thông tin người dùng
            GoogleJsonWebSignature.Payload payload = await VerifyGoogleToken(googleToken) ?? throw new UnauthorizedAccessException("Invalid Google token.");

            if (!payload.EmailVerified)
            {
                throw new UnauthorizedAccessException("Email is not verified. Please verify your email to access our website");
            }

            string? givenName = payload.GivenName; // Given Name can be null
            string? surName = payload.FamilyName; // Sur Name can be null
            string? fullName = Util.ValidateAndCombineName(givenName, surName);

            // Lấy URL ảnh người dùng
            string avatar = payload.Picture;

            // Lấy Email người dùng
            string email = payload.Email;

            // Lấy thông tin người dùng
            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.Email == email).FirstOrDefaultAsync();

            // Lấy thông tin ảnh từ URL
            (int? imageHeight, int? imageWidth) = await Util.GetImageInfoFromUrlSkiaSharp(avatar);

            // Chỉ khi nào chạy deploy thì mới sử dụng hàm này
            //GeolocationResponseModel geolocationResponseModel = await _geolocation.GetLocationFromHeaderAsync();

            // Trường hợp mới đăng nhập google account lần đầu tức là chưa tồn tại tài khoản trong db
            if (retrieveUser is null)
            {
                retrieveUser = new User()
                {
                    DisplayName = fullName ?? "Ẩn Danh",
                    Email = email,
                    Gender = UserGender.NotSpecified,
                    Images =
                    [
                        new()
                        {
                            URL = avatar,
                            Height = imageHeight ?? 96,
                            Width = imageWidth ?? 96,
                        },
                    ],
                    Roles = [UserRole.Customer],
                    Product = UserProduct.Free,
                    CountryId = "VN",
                    Status = UserStatus.Active,
                    IsLinkedWithGoogle = true,
                    CreatedTime = Util.GetUtcPlus7Time()
                };
                await _unitOfWork.GetCollection<User>().InsertOneAsync(retrieveUser);
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

            // Có thể không cần dùng claimList vì trên đó đã có list về claim và tùy theo hệ thống nên tạo mới list claim
            IEnumerable<Claim> claimsList =
            [
                new Claim(ClaimTypes.NameIdentifier, retrieveUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, payload.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, payload.GivenName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, payload.FamilyName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, payload.Name),
                new Claim("Avatar", payload.Picture),
                new Claim(ClaimTypes.Role, "Customer")
            ];

            // Gọi phương thức để tạo access token và refresh token từ danh sách claim và thông tin người dùng
            _jwtBLL.GenerateAccessToken(claimsList, retrieveUser.Id, out string accessToken, out string refreshToken);

            // Đảm bảo rằng hệ thống đã tạo AccessToken thành công thì mới cập nhật field LastLoginTime
            UpdateDefinition<User> updateDefinition = Builders<User>.Update.Set(user => user.LastLoginTime, Util.GetUtcPlus7Time());
            await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == retrieveUser.Id, updateDefinition);

            // New object ModelView
            AuthenticatedResponseModel authenticationModel = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return authenticationModel;
        }

        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string googleToken)
        {
            GoogleJsonWebSignature.ValidationSettings settings = new()
            {
                Audience = [Environment.GetEnvironmentVariable("Authentication_Google_ClientId")]
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
            return payload;
        }

        // Khi người dùng xác nhận liên kết tài khoản Google FE sẽ gọi API này
        public async Task<AuthenticatedResponseModel> ConfirmLinkWithGoogleAccount(string email)
        {
            // Lấy thông tin người dùng
            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.Email == email && user.IsLinkedWithGoogle == false).FirstOrDefaultAsync() ?? throw new CustomException("Google Account Linking", 44, "Not found user or user has been linked with google account");

            // Cập nhật trạng thái liên kết tài khoản Google
            UpdateDefinition<User> isLinkedWithGoogleUpdate = Builders<User>.Update.Set(user => user.IsLinkedWithGoogle, true);
            UpdateResult isLinkedWithGoogleUpdateResult = await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == retrieveUser.Id, isLinkedWithGoogleUpdate);

            // Kiểm tra nếu có ít nhất một field_name được cập nhật
            if (isLinkedWithGoogleUpdateResult.ModifiedCount < 1)
            {
                throw new CustomException("Update Fail", StatusCodes.Status500InternalServerError, "Found user but can not update");
            }

            // Claim list
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, retrieveUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, retrieveUser.Roles.ToString()),
                    new Claim(ClaimTypes.Name, retrieveUser.DisplayName),
                    new Claim("Avatar", retrieveUser.Images[0].URL)
                ];

            // Gọi phương thức để tạo access token và refresh token từ danh sách claim và thông tin người dùng
            _jwtBLL.GenerateAccessToken(claims, retrieveUser.Id, out string accessToken, out string refreshToken);

            // New object ModelView
            AuthenticatedResponseModel authenticationModel = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return authenticationModel;
        }

        public async Task<AuthenticatedUserInfoResponseModel> Authenticate(LoginRequestModel loginModel)
        {
            string username = loginModel.Username;
            string password = loginModel.Password;

            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.UserName == username).FirstOrDefaultAsync() ?? throw new ArgumentException("Username or Password is incorrect");

            bool isPasswordHashed = BCrypt.Net.BCrypt.Verify(loginModel.Password, retrieveUser.Password);
            if (!isPasswordHashed)
            {
                throw new ArgumentException("Username or Password is incorrect");
            }

            switch (retrieveUser.Status)
            {
                case UserStatus.Inactive:
                    _httpContextAccessor.HttpContext?.Session.SetString("UserNameTemp", retrieveUser.UserName);
                    throw new UnAuthorizedCustomException("Not active");
                case UserStatus.Banned: throw new UnAuthorizedCustomException("Banned");
            }

            // JWT
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, retrieveUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, retrieveUser.Roles[0].ToString()),
                    new Claim(ClaimTypes.Name, retrieveUser.DisplayName),
                    new Claim("Avatar", retrieveUser.Images[0].URL)
                ];

            //Call method to generate access token
            _jwtBLL.GenerateAccessToken(claims, retrieveUser.Id, out string accessToken, out string refreshToken);

            //if (accessToken is null)
            //{
            //    await Console.Out.WriteLineAsync("accesstoken is null");
            //}

            // New object ModelView
            //AuthenticatedResponseModel authenticationModel = new()
            //{
            //    AccessToken = accessToken,
            //    RefreshToken = refreshToken
            //};

            CookieOptions cookieOptions = new()
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(7)
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("SpotifyPool_RefreshToken", refreshToken, cookieOptions);

            AuthenticatedUserInfoResponseModel authenticatedUserInfoResponseModel = new()
            {
                AccessToken = accessToken,
                Id = retrieveUser.Id.ToString(),
                Name = retrieveUser.DisplayName,
                Role = [retrieveUser.Roles[0].ToString()],
                Avatar = [retrieveUser.Images[0].URL]
            };

            // Update LastLoginTime
            // Chỗ này chưa cần phải kiểm tra Update Result
            // Nếu chỗ này throw exception thì người dùng sẽ không login được
            UpdateDefinition<User> lastLoginTimeUpdate = Builders<User>.Update.Set(user => user.LastLoginTime, Util.GetUtcPlus7Time());
            await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == retrieveUser.Id, lastLoginTimeUpdate);

            return authenticatedUserInfoResponseModel;
        }

        public async Task<AuthenticatedResponseModel> SwitchProfile()
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Projection
            ProjectionDefinition<ASUser> projectionDefinition = Builders<ASUser>.Projection
                .Include(user => user.Id)
                .Include(user => user.DisplayName)
                .Include(user => user.Images)
                .Include(user => user.Artist);

            // Stage
            IAggregateFluent<User> aggregateFluent = _unitOfWork.GetCollection<User>().Aggregate();

            // Lookup
            ASUser userArtist = await aggregateFluent
                .Match(user => user.Id == userID)
                .Lookup<User, Artist, ASUser>
                (
                    _unitOfWork.GetCollection<Artist>(),
                    user => user.Id,
                    artist => artist.UserId,
                    result => result.Artist
                )
                .Unwind(x => x.Artist, new AggregateUnwindOptions<ASUser> { PreserveNullAndEmptyArrays = true })
                .Project(Builders<ASUser>.Projection
                .Include(user => user.Id)
                .Include(user => user.DisplayName)
                .Include(user => user.Images)
                .Include(user => user.Artist.Id)
                .Include(user => user.Artist.Name)
                .Include(user => user.Artist.Images))
                .As<ASUser>()
                .FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("You do not have an artist account!");

            // Lấy role hiện tại từ claims
            string currentRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? throw new DataNotFoundCustomException("Not found role");

            // Parse role
            UserRole userRole = Enum.Parse<UserRole>(currentRole);

            // Khởi tạo biến displayName và avatar
            string displayName;
            string avatar;

            // Kiểm tra role người dùng
            if (userRole == UserRole.Customer)
            {
                displayName = userArtist.Artist.Name;
                userRole = UserRole.Artist;
                avatar = userArtist.Artist.Images[0].URL;
            }
            else
            {
                displayName = userArtist.DisplayName;
                userRole = UserRole.Customer;
                avatar = userArtist.Images[0].URL;
            }

            // Claim list
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, userArtist.Id.ToString()),
                    new Claim(ClaimTypes.Role, userRole.ToString()),
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim("Avatar", avatar)
                ];

            // Gọi phương thức để tạo access token và refresh token từ danh sách claim và thông tin người dùng
            _jwtBLL.GenerateAccessToken(claims, userID, out string accessToken, out string refreshToken);

            // New object ModelView
            AuthenticatedResponseModel authenticationModel = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return authenticationModel;
        }

        public async Task ReActiveAccountByToken()
        {
            string username = _httpContextAccessor.HttpContext.Session.GetString("UserNameTemp");
            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.UserName == username).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("No username found in session. Please log in again.");

            UserResponseModel userResponseModel = _mapper.Map<UserResponseModel>(retrieveUser);

            string email = retrieveUser.Email;

            string encryptedToken = DataEncryptionExtensions.HmacSHA256(email, Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Mode property is not set in environment or not found"));

            string token = _jwtBLL.GenerateJWTTokenForConfirmEmail(email, encryptedToken);
            string confirmationLink = $"http://localhost:5173/spotifypool/confirm-email?token={token}";

            UpdateDefinition<User> tokenUpdate = Builders<User>.Update.Set(user => user.TokenEmailConfirm, encryptedToken);
            UpdateResult tokenResult = await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == retrieveUser.Id, tokenUpdate);

            if (tokenResult.ModifiedCount < 1)
            {
                throw new CustomException("Update Fail", StatusCodes.Status500InternalServerError, "Found user but can not update");
            }

            EmailSenderRequestModel emailSenderRequestModel = new()
            {
                EmailTo = [email],
                Subject = "Xác nhận Email"
            };


            return;
        }

        public async Task SendOTPForgotPasswordAsync(ForgotPasswordRequestModel model)
        {
            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.Email == model.Email).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("There's no account match with this email!");
            string otpToEmail = await CreateOTPAsync(retrieveUser.Email);

            EmailSenderRequestModel emailSenderRequestModel = new()
            {
                EmailTo = [retrieveUser.Email],
                Subject = "OTP forgot password"
            };

            //tạo model chứa thông tin cần thiết để gửi email
            EmailMetadata emailMetadata = new(retrieveUser.Email, "OTP forgot password", otpToEmail);

            // Gửi email
            await _emailService.SendAsync(emailMetadata, 2);
        }

        public async Task ValidateOTPPassword(string email, string otpCode)
        {
            OTP otp = await _unitOfWork.GetCollection<OTP>().Find(otp => otp.Email == email && otp.OTPCode == otpCode)
                                         .FirstOrDefaultAsync()
                    ?? throw new DataNotFoundCustomException("OTP is not correct!");

            if (otp.ExpiryTime < DateTimeOffset.UtcNow)
            {
                throw new BadRequestCustomException("OTP has expired!");
            }

            UpdateDefinition<OTP> update = Builders<OTP>.Update.Set(otp => otp.IsUsed, true);
            await _unitOfWork.GetCollection<OTP>().UpdateOneAsync(otp => otp.Email == email, update);

            User retrieveUser = await _unitOfWork.GetCollection<User>().Find(user => user.Email == email).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("There's no account match with this email!");

            //set lại mk mặc định, lấy 6 kí tự cuối theo thời gian hiện tại 
            string password = "SpotifyPool" + DateTimeOffset.UtcNow.ToString("ffffff");

            UpdateDefinition<User> updatePassword = Builders<User>.Update.Set(user => user.Password, BCrypt.Net.BCrypt.HashPassword(password));
            await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Email == email, updatePassword);


            //tạo model chứa thông tin cần thiết để gửi email
            EmailMetadata emailMetadata = new(retrieveUser.Email, "Reset Password", password);

            // Gửi email
            await _emailService.SendAsync(emailMetadata, 3);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestModel model)
        {
            string userName = _httpContextAccessor.HttpContext.Session.GetString("UserName"); // chỗ này lấy từ login

			if (model.NewPassword != model.ConfirmPassword)
            {
                throw new BadRequestCustomException("Confirm password does not match with new password!");
            }

            // reset password
            UpdateDefinition<User> update = Builders<User>.Update.Set(user => user.Password, BCrypt.Net.BCrypt.HashPassword(model.NewPassword));
            await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.UserName == userName, update);
        }

        private async Task<string> CreateOTPAsync(string email)
        {
            string otpCode = Util.GenerateOTP();
            //user đã có otp thì update lại, chưa thì tạo mới
            if (await _unitOfWork.GetCollection<OTP>().Find(otp => otp.Email == email).AnyAsync())
            {
                UpdateDefinition<OTP> update = Builders<OTP>.Update.Set(otp => otp.OTPCode, otpCode)
                                                                   .Set(otp => otp.IsUsed, false)
                                                                   .Set(otp => otp.ExpiryTime, DateTimeOffset.UtcNow.AddMinutes(5));
                await _unitOfWork.GetCollection<OTP>().UpdateOneAsync(otp => otp.Email == email, update);
            }
            else
            {
                OTP otp = new()
                {
                    Email = email,
                    OTPCode = otpCode,
                    ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5)
                };
                //Console.WriteLine(otp.ExpiryTime.LocalDateTime);
                await _unitOfWork.GetCollection<OTP>().InsertOneAsync(otp);
            }
            return otpCode;
        }

        public AuthenticatedUserInfoResponseModel GetUserInformation(string token)
        {
            List<Claim> info = _jwtBLL.ValidateToken(token).Claims.ToList();

            // lấy thông tin người dùng từ token
            AuthenticatedUserInfoResponseModel userinfo = new()
            {
                Id = info.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                ArtistId = info.FirstOrDefault(c => c.Type == "ArtistId")?.Value ?? null,
                Name = info.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Role = info.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                Avatar = info.Where(c => c.Type == "Avatar").Select(c => c.Value).ToList()
            };
            return userinfo;
        }

        public AuthenticatedUserInfoResponseModel Relog()
        {
            // lấy refresh token từ cookie
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("SpotifyPool_RefreshToken", out string refreshTokenValue);


            // gọi hàm để gen lại access token & refresh token 
            _jwtBLL.RefreshAccessToken(out string accessToken, out string RefreshToken, out ClaimsPrincipal principal, refreshTokenValue);

            AuthenticatedUserInfoResponseModel authenticatedUserInfoResponseModel = new()
            {
                AccessToken = accessToken,
                Id = principal.Identity?.Name,
                ArtistId = principal.FindFirst("ArtistId")?.Value ?? null,
                Name = principal.FindFirst(ClaimTypes.Name)?.Value,
                Role = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                Avatar = principal.FindAll("Avatar").Select(c => c.Value).ToList()
            };

            CookieOptions cookieOptions = new()
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(7)
            };

            // cập nhật lại refresh token mới vào cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("SpotifyPool_RefreshToken");
            _httpContextAccessor.HttpContext.Response.Cookies.Append("SpotifyPool_RefreshToken", RefreshToken, cookieOptions);

            return authenticatedUserInfoResponseModel;
        }

        public async Task LogoutAsync()
        {
            //xóa refresh token trong cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("SpotifyPool_RefreshToken");

            //chỗ này nếu logout đúng lúc hết hạn thì không cần xóa key refresh trong redis
            string? userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(userId is not null)
            {
                //xóa key refresh trong redis để dọn dẹp tài nguyên ko cần thiết vì token 7 ngày lận
                await _redis.KeyDeleteAsync(userId);
            }
            return;
        }
    }
}
