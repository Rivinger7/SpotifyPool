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
using System.Web;
using System.Text.RegularExpressions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using CloudinaryDotNet.Actions;

namespace BusinessLogicLayer.Implement.Services.Users
{
    public class UserBLL(IUnitOfWork unitOfWork, IMapper mapper, ICacheCustom cache, IHttpContextAccessor httpContextAccessor, CloudinaryService cloudinaryService) : IUserBLL
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICacheCustom _cache = cache;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;

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

		public async Task EditProfileAsync(EditProfileRequestModel requestModel)
		{
			string userName = _httpContextAccessor.HttpContext.Session.GetString("UserName");

			if (string.IsNullOrEmpty(userName))
			{
				throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
			}


			User user = await _unitOfWork.GetCollection<User>()
				.Find(user => user.UserName == userName)
				.FirstOrDefaultAsync();

			// nếu không điền dữ liệu mới thì lấy lại cái cũ
			requestModel.DisplayName = requestModel.DisplayName ?? user.DisplayName;
			requestModel.FullName = requestModel.FullName ?? user.FullName;
			requestModel.PhoneNumber = requestModel.PhoneNumber ?? user.PhoneNumber;
			requestModel.Birthdate = requestModel.Birthdate ?? user.Birthdate;
			requestModel.Gender = requestModel.Gender ?? user.Gender;

			// tạo cập nhật field khác trc
			UpdateDefinition<User> update = Builders<User>.Update
				.Set(u => u.FullName, requestModel.FullName)
				.Set(u => u.DisplayName, requestModel.DisplayName)
				.Set(u => u.PhoneNumber, requestModel.PhoneNumber)
				.Set(u => u.Birthdate, requestModel.Birthdate)
				.Set(u => u.Gender, requestModel.Gender);

			if (requestModel.Image is not null)
			{
				string linkImage = user.Images.Last().URL;

				// decode từ URL 
				string textNormalizedFromUrl = HttpUtility.UrlDecode(linkImage);

				// regex để lấy publicID
				Regex regex = new Regex(@"User's Profiles\/([a-zA-Z0-9%_=]+)\.webp$");
				Match match = regex.Match(textNormalizedFromUrl);

				string publicIDImage = match.Groups[1].Value;

				// Xóa ảnh cũ nếu ảnh không phải ảnh mặc định
				if (publicIDImage != "RaQXMK0XJlX0bZbUyHcSfA%3D%3D_638647809024468188")
				{
					_cloudinaryService.DeleteImage(publicIDImage);
				}

				ImageUploadResult result = _cloudinaryService.UploadImage(requestModel.Image);

				user.Images.Clear();
				user.Images.Add(new Image { URL = result.SecureUrl.ToString(), Height = 500, Width = 313 });

				// cập nhật danh sách ảnh mới
				update = update.Set(u => u.Images, user.Images);
			}


			await _unitOfWork.GetCollection<User>()
				.UpdateOneAsync(user => user.UserName == userName, update);
		}
	}
}
