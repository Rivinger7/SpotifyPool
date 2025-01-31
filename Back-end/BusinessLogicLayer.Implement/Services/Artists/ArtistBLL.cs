using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Artists;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.User;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.Artists
{
    public class ArtistBLL(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService, IHttpContextAccessor httpContextAccessor, IJwtBLL jwtBLL) : IArtist
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IJwtBLL _jwtBLL = jwtBLL;

        public async Task CreateArtist(ArtistRequest artistRequest)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Thêm role user vào user
            UpdateDefinition<User> updateDefinition = Builders<User>.Update.AddToSet(u => u.Roles, UserRole.Artist);
            UpdateResult updateResult = await _unitOfWork.GetCollection<User>().UpdateOneAsync(u => u.Id == userID, updateDefinition);

            // Kiểm tra role đã được thêm vào user chưa
            if (updateResult.ModifiedCount < 1)
            {
                throw new InternalServerErrorCustomException("Can't add user role to user!");
            }

            // Kiểm tra xem account đã là nghệ sĩ chưa
            Artist artist = await _unitOfWork.GetCollection<Artist>().Find(a => a.UserId == userID).FirstOrDefaultAsync();
            if (artist is not null)
            {
                throw new DataExistCustomException("You are already have an user account!");
            }

            // Khởi tạo danh sách hình ảnh
            List<Image> images = [];

            // Nếu có file hình ảnh thì upload lên Cloudinary
            // Gọi 3 lần để tạo 3 kích thước ảnh khác nhau
            if (artistRequest.ImageFile != null)
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh
                IEnumerable<int> sizes = [640, 300, 64];
                // Kích thước ảnh cố định
                int fixedSize = 300;

                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(artistRequest.ImageFile, ImageTag.Artist, rootFolder: "Image", fixedSize, fixedSize);

                // Tạo 3 kích thước ảnh khác nhau nhưng cùng một URL với kích thước cố định
                foreach (int size in sizes)
                {
                    images.Add(new()
                    {
                        URL = uploadResult.SecureUrl.AbsoluteUri,
                        Height = size,
                        Width = size
                    });
                }
            }

            // Tạo mới account nghệ sĩ
            artist = new()
            {
                Name = artistRequest.Name,
                UserId = userID,
                Images = images,
            };

            // Lưu thông tin nghệ sĩ vào database
            await _unitOfWork.GetCollection<Artist>().InsertOneAsync(artist);
        }

        public async Task<AuthenticatedResponseModel> SwitchToUserProfile()
        {
            // Lấy UserId từ phiên người dùng
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Lấy thông tin user
            User user = await _unitOfWork.GetCollection<User>().Find(a => a.Id == userID).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("Not found any user!");

            // Claim list
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, UserRole.Customer.ToString()),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim("Avatar", user.Images[0].URL ?? throw new DataNotFoundCustomException("Not found any images"))
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
    }
}
