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

namespace BusinessLogicLayer.Implement.Services.Users
{
    public class UserBLL(IUnitOfWork unitOfWork, IMapper mapper, ICacheCustom cache, IHttpContextAccessor httpContextAccessor, CloudinaryService cloudinaryService) : IUserBLL
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICacheCustom _cache = cache;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;

        public async Task<IEnumerable<UserResponseModel>> GetAllUsersAsync(string? displayName, string? gender, string? email, bool isCache = false)
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
            if (!string.IsNullOrEmpty(gender))
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
                    DisplayName = users.DisplayName,
                    Gender = users.Gender,
                    Birthdate = users.Birthdate,
                    //ImageResponseModel = users.ImageResponseModel,
                    IsLinkedWithGoogle = users.IsLinkedWithGoogle,
                    Status = users.Status.ToString(),
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
                    DisplayName = users.DisplayName,
                    Gender = users.Gender,
                    Birthdate = users.Birthdate,
                    //ImageResponseModel = users.ImageResponseModel,
                    IsLinkedWithGoogle = users.IsLinkedWithGoogle,
                    Status = users.Status.ToString()
                })
                .ToListAsync();
            }

            return users;
        }


        public async Task<UserResponseModel> GetUserByIDAsync(string id, bool isCache = false)
        {
            //ObjectId objectId = ObjectId.Parse(id);

            UserResponseModel user;

            if (isCache)
            {
                user = await _cache.GetOrSetAsync(id.ToString(), () => _unitOfWork.GetCollection<User>().Find(user => user.Id == id).Project(user => new UserResponseModel
                {
                    UserId = user.Id.ToString(),
                    Role = user.Role.ToString(),
                    DisplayName = user.DisplayName,
                    Gender = user.Gender,
                    Birthdate = user.Birthdate,
                    //ImageResponseModel = user.Images,
                    IsLinkedWithGoogle = user.IsLinkedWithGoogle,
                    Status = user.Status.ToString(),
                }).FirstOrDefaultAsync());
            }
            else
            {
                user = await _unitOfWork.GetCollection<User>().Find(user => user.Id == id).Project(user => new UserResponseModel
                {
                    UserId = user.Id.ToString(),
                    Role = user.Role.ToString(),
                    DisplayName = user.DisplayName,
                    Gender = user.Gender,
                    Birthdate = user.Birthdate,
                    //ImageResponseModel = user.ImageResponseModel,
                    IsLinkedWithGoogle = user.IsLinkedWithGoogle,
                    Status = user.Status.ToString()

                }).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException($"Not found User with ID {id}");
            }

            //UserResponseModel userResponseModel = _mapper.Map<UserResponseModel>(user);

            return user;
        }

        public async Task EditProfileAsync(EditProfileRequestModel requestModel)
        {
            string userId = _httpContextAccessor.HttpContext.Session.GetString("UserID");

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
            requestModel.Gender ??= user.Gender;

            //map từ model qua user
            _mapper.Map<EditProfileRequestModel, User>(requestModel, user);

            // Cập nhật Image Field nếu có
            UpdateDefinition<User>? updateDefinition = null;
            if (requestModel.Image is not null)
            {
                ImageUploadResult result = _cloudinaryService.UploadImage(requestModel.Image, ImageTag.Users_Profile);
                updateDefinition = Builders<User>.Update.Set(user => user.Images.First().URL, result.SecureUrl.ToString());
            }

            // Cập nhật các fields khác
            updateDefinition.Set(user => user.DisplayName, requestModel.DisplayName)
                    .Set(user => user.PhoneNumber, requestModel.PhoneNumber)
                    .Set(user => user.Birthdate, requestModel.Birthdate)
                    .Set(user => user.Gender, requestModel.Gender);
            UpdateResult updateResult = await _unitOfWork.GetCollection<User>().UpdateOneAsync(user => user.Id == user.Id, updateDefinition);
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

            var result = await _unitOfWork.GetRepository<User>().Paging(offset, limit);
            return _mapper.Map<IReadOnlyCollection<UserResponseModel>>(result);
        }
    }
}
