using AutoMapper;
using Business_Logic_Layer.Services_Interface.InMemoryCache;
using Business_Logic_Layer.Services_Interface.Users;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using BusinessLogicLayer.Implement.CustomExceptions;
using DataAccessLayer.Interface.MongoDB.UOW;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using Microsoft.AspNetCore.Http;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using CloudinaryDotNet.Actions;
using SetupLayer.Enum.Services.User;
using SetupLayer.Enum.Microservices.Cloudinary;
using Utility.Coding;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.Users
{
    public class UserBLL(IUnitOfWork unitOfWork, IMapper mapper, ICacheCustom cache, IHttpContextAccessor httpContextAccessor, CloudinaryService cloudinaryService) : IUserBLL
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICacheCustom _cache = cache;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;

        public async Task<IEnumerable<UserResponseModel>> GetAllUsersAsync(string? displayName, UserGender? gender, string? email, bool isCache = false)
        {
            // Khởi tạo bộ lọc (trống ban đầu)
            var filterBuilder = Builders<User>.Filter;
            var filter = filterBuilder.Empty;

            // Xây dựng cacheKey dựa trên các bộ lọc
            string cacheKey = "users";

            // Kiểm tra và thêm điều kiện lọc cho displayName nếu không null hoặc rỗng
            if (!string.IsNullOrEmpty(displayName))
            {
                // 'i' là ignore case
                filter &= filterBuilder.Regex(u => u.DisplayName, new BsonRegularExpression(displayName, "i"));
                cacheKey += $"_fullname_{displayName}";
            }

            // Kiểm tra và thêm điều kiện lọc cho gender nếu không null hoặc rỗng
            if (!string.IsNullOrEmpty(gender.ToString()))
            {
                filter &= filterBuilder.Eq(u => u.Gender, gender);
                cacheKey += $"_gender_{gender}";
            }

            // Kiểm tra và thêm điều kiện lọc cho email nếu không null hoặc rỗng
            if (!string.IsNullOrEmpty(email))
            {
                // 'i' là ignore case
                filter &= filterBuilder.Regex(u => u.Email, new BsonRegularExpression(email, "i"));
                cacheKey += $"_email_{email}";
            }

            // Lấy tất cả thông tin người dùng và áp dụng bộ lọc
            IEnumerable<UserResponseModel> users;
            if (isCache)
            {
                users = await _cache.GetOrSetAsync(cacheKey, () => _unitOfWork.GetCollection<User>().Find(filter)
                .Project(users => new UserResponseModel
                {
                    UserId = users.Id.ToString(),
                    Role = users.Role.ToString(),
                    Email = users.Email,
                    DisplayName = users.DisplayName,
                    Gender = users.Gender.ToString(),
                    Birthdate = users.Birthdate,
                    //ImageResponseModel = users.ImageResponseModel,
                    IsLinkedWithGoogle = users.IsLinkedWithGoogle,
                    Status = users.Status.ToString(),
                    CreatedTime = Util.GetUtcPlus7Time().ToString("yyyy-MM-dd")
                })
                .ToListAsync());
            }
            else
            {
                users = await _unitOfWork.GetCollection<User>().Find(filter)
                .Project(users => new UserResponseModel
                {
                    UserId = users.Id.ToString(),
                    Role = users.Role.ToString(),
                    Email = users.Email,
                    DisplayName = users.DisplayName,
                    Gender = users.Gender.ToString(),
                    Birthdate = users.Birthdate,
                    //ImageResponseModel = users.ImageResponseModel,
                    IsLinkedWithGoogle = users.IsLinkedWithGoogle,
                    Status = users.Status.ToString(),
                    CreatedTime = users.CreatedTime.ToString(),
                    LastLoginTime = users.LastLoginTime.HasValue ? users.LastLoginTime.Value.ToString() : null,
                    LastUpdatedTime = users.LastUpdatedTime.HasValue ? users.LastUpdatedTime.Value.ToString() : null,
                })
                .ToListAsync();
            }

            return users;
        }


        public async Task<UserResponseModel> GetUserByIDAsync(string id)
        {
            // Projection
            ProjectionDefinition<User> userProjection = Builders<User>.Projection
                .Include(user => user.Id)
                .Include(user => user.DisplayName)
                .Include(user => user.Images);

            // Lấy thông tin người dùng theo ID
            User user = await _unitOfWork.GetCollection<User>().Find(user => user.Id == id)
                .Project<User>(userProjection)
                .FirstOrDefaultAsync();

            // Mapping từ User sang UserResponseModel
            UserResponseModel userResponseModel = _mapper.Map<UserResponseModel>(user);

            return userResponseModel;
        }

        public async Task EditProfileAsync(EditProfileRequestModel requestModel)
        {
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            User user = await _unitOfWork.GetCollection<User>()
                                         .Find(user => user.Id == userId)
                                         .Project(Builders<User>.Projection.Include(user => user.Images))
                                         .As<User>()
                                         .FirstOrDefaultAsync()
                        ?? throw new DataNotFoundCustomException("Not found any user");

            // nếu không điền dữ liệu mới thì lấy lại cái cũ
            requestModel.DisplayName ??= user.DisplayName;
            requestModel.PhoneNumber ??= user.PhoneNumber;
            requestModel.Birthdate ??= user.Birthdate;
            requestModel.Gender ??= user.Gender.ToString();

            //map từ model qua user
            _mapper.Map<EditProfileRequestModel, User>(requestModel, user);

            // Cập nhật các field sẵn có
            UpdateDefinition<User> updateDefinition = Builders<User>.Update.Set(user => user.DisplayName, requestModel.DisplayName)
                    .Set(user => user.PhoneNumber, requestModel.PhoneNumber)
                    .Set(user => user.Birthdate, requestModel.Birthdate)
                    .Set(user => user.Gender, Enum.Parse<UserGender>(requestModel.Gender))
                    .Set(user => user.LastUpdatedTime, Util.GetUtcPlus7Time());

			// Cập nhật Image Field nếu có
			if (requestModel.Image is not null)
            {
                ImageUploadResult result = _cloudinaryService.UploadImage(requestModel.Image, ImageTag.Users_Profile);
                updateDefinition = updateDefinition.Set(user => user.Images.First().URL, result.SecureUrl.ToString());
            }

            await _unitOfWork.GetRepository<User>().UpdateAsync(user.Id, updateDefinition);
        }

        //test method
        public async Task<IEnumerable<UserResponseModel>> TestPaging(int offset, int limit)
        {

            IMongoCollection<User> collection = _unitOfWork.GetCollection<User>();

            FilterDefinition<User> filter = Builders<User>.Filter.Eq(user => user.Status, UserStatus.Active);

            ////BONUS thêm cách dùng điều kiện
            //FilterDefinitionBuilder<User> builder = Builders<User>.Filter;

            //FilterDefinition<User> filter = builder.Empty;

            //builder.Or(builder.Eq(user => user.FullName, "DuyHoang"), builder.Eq(user => user.UserName, "phuchoa"));


            ////*** nếu muốn có sort thì thêm SortDefinition, như lày
            // SortDefinition<User> sort = Builders<User>.Sort.Descending(user => user.FullName);

            IEnumerable<User> result = await _unitOfWork.GetRepository<User>().Paging(offset, limit);
            return _mapper.Map<IReadOnlyCollection<UserResponseModel>>(result);
        }
    }
}
