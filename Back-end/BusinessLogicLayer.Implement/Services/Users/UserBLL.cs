using AutoMapper;
using Business_Logic_Layer.Services_Interface.InMemoryCache;
using Business_Logic_Layer.Services_Interface.Users;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.ModelView.Service_Model_Views;
using DataAccessLayer.Interface.MongoDB.UOW;

namespace BusinessLogicLayer.Implement.Services.Users
{
    public class UserBLL(IUnitOfWork unitOfWork, IMapper mapper, ICacheCustom cache) : IUserBLL
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICacheCustom _cache = cache;

        public async Task<IEnumerable<UserResponseModel>> GetAllUsersAsync(string? fullname, string? gender, string? email, bool isCache = false)
        {
            // Khởi tạo bộ lọc (trống ban đầu)
            var filterBuilder = Builders<User>.Filter;
            var filter = filterBuilder.Empty;

            // Xây dựng cacheKey dựa trên các bộ lọc
            string cacheKey = "users";

            // Kiểm tra và thêm điều kiện lọc cho fullname nếu không null hoặc rỗng
            if (!string.IsNullOrEmpty(fullname))
            {
                // 'i' là ignore case
                filter &= filterBuilder.Regex(u => u.FullName, new BsonRegularExpression(fullname, "i"));
                cacheKey += $"_fullname_{fullname}";
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
                    Role = users.Role,
                    FullName = users.FullName,
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
                    Role = users.Role,
                    FullName = users.FullName,
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
                    Role = user.Role,
                    FullName = user.FullName,
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
                    Role = user.Role,
                    FullName = user.FullName,
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
    }
}
