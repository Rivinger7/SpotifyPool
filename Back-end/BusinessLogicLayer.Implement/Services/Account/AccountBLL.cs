using AutoMapper;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Account;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.User;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Account
{
	public class AccountBLL(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService) : IAccount
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IMapper _mapper = mapper;
		private readonly CloudinaryService _cloudinaryService = cloudinaryService;

		#region Hiển thị thông tin tất cả người dùng (GetPaging)
		public async Task<IEnumerable<AccountResponse>> GetPaggingAsync(PagingRequestModel request, AccountFilterModel model)
		{
			//Tạo danh sách các đk lọc
			List<FilterDefinition<User>> filters = new List<FilterDefinition<User>>();

			//Lọc chỉ thấy Customer và Artist
			filters.Add(Builders<User>.Filter.AnyIn(u => u.Roles, new[] { UserRole.Customer, UserRole.Artist }));

			//Search theo UerName
			if (!string.IsNullOrWhiteSpace(model.UserName))
			{
				filters.Add(Builders<User>.Filter.Regex(u => u.UserName, new BsonRegularExpression(model.UserName, "i")));
			}

			//Search theo Email
			if (!string.IsNullOrWhiteSpace(model.Email))
			{
				filters.Add(Builders<User>.Filter.Regex(u => u.Email, new BsonRegularExpression(model.Email, "i")));
			}

			//Lọc theo Status
			if (model.Status.HasValue)
			{
				filters.Add(Builders<User>.Filter.Eq(u => u.Status, model.Status.Value));
			}

			//Kết hợp all filter bằng And()
			FilterDefinition<User> combinedFilter = filters.Count > 0
				? Builders<User>.Filter.And(filters)
				: Builders<User>.Filter.Empty;

			var query = _unitOfWork.GetCollection<User>()
				.Find(combinedFilter);

			//Sort theo DisplayName (Nếu có)
			if (model.DisplayName.HasValue)
			{
				query = model.DisplayName.Value
					? query.SortBy(u => u.DisplayName)   //Tăng dần
					: query.SortByDescending(u => u.DisplayName); //Giảm dần
			}
			else
			{
				//Sort theo CreateTime giảm dần (tài khoản nào mới tạo sẽ được đưa lên đầu)
				query = query.SortByDescending(i => i.CreatedTime);
			}

			IEnumerable<User> result = await query
				.Skip((request.PageNumber - 1) * request.PageSize) //Phân trang
				.Limit(request.PageSize) //Giới hạn số lượng
				.ToListAsync();

			return _mapper.Map<IReadOnlyCollection<AccountResponse>>(result);
		}
		#endregion

		#region Hiển thị đầy đủ thông tin người dùng (GetById)
		public async Task<AccountDetailResponse> GetByIdAsync(string id)
		{
			//Lấy thông tin người dùng theo ID
			User user = await _unitOfWork.GetCollection<User>()
				.Find(user => user.Id == id)
				.FirstOrDefaultAsync();

			//Mapping từ User sang AccountDetailResponse
			AccountDetailResponse accountResponse = _mapper.Map<AccountDetailResponse>(user);

			return accountResponse;
		}
		#endregion

		#region Thêm tài khoản người dùng (Create)
		public async Task CreateAsync(CreateRequestModel userRequest)
		{
			//Check xác nhận mật khẩu
			if (userRequest.Password != userRequest.ConfirmedPassword)
			{
				throw new ArgumentException("Confirmation password does not match.");
			}

			string? imageUrl = null;

			//Nếu có tải ảnh lên
			if (userRequest.Image is not null)
			{
				// Upload ảnh lên Cloudinary
				ImageUploadResult result = _cloudinaryService.UploadImage(userRequest.Image, ImageTag.Users_Profile);

				//Gán URL của ảnh upload vào biến imageUrl
				imageUrl = result.SecureUrl.AbsoluteUri;
			}

			//Nếu không tải ảnh, dùng ảnh mặc định
			if (string.IsNullOrEmpty(imageUrl))
			{
				Console.WriteLine("Using default image.");
				imageUrl = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779869/default-playlist-640_tsyulf.jpg";
			}

			//Tạo danh sách 3 kích thước ảnh
			List<Image> images = new List<Image>
			{
				new Image { URL = imageUrl, Height = 640, Width = 640 },
				new Image { URL = imageUrl, Height = 300, Width = 300 },
				new Image { URL = imageUrl, Height = 64, Width = 64 }
			};

			//Map từ CreateRequestModel sang User
			User user = new User
			{
				UserName = userRequest.UserName,
				Password = BCrypt.Net.BCrypt.HashPassword(userRequest.Password), //Mã hóa mật khẩu
				DisplayName = userRequest.DisplayName,
				Email = userRequest.Email,
				PhoneNumber = userRequest.PhoneNumber,
				Roles = userRequest.Roles,
				Status = UserStatus.Active,
				Images = images,
				CreatedTime = Util.GetUtcPlus7Time()
			};

			//Lưu vào MongoDB
			await _unitOfWork.GetCollection<User>().InsertOneAsync(user);
		}
		#endregion

		#region Chỉnh sửa thông tin người dùng (Update)
		public async Task UpdateByIdAsync(string id, UpdateRequestModel userRequest)
		{
			FilterDefinition<User> filter = Builders<User>.Filter.Eq(u => u.Id, id);

			//Lấy User hiện tại
			User existingUser = await _unitOfWork.GetCollection<User>()
				.Find(u => u.Id == id)
				.FirstOrDefaultAsync()
				?? throw new KeyNotFoundException($"User with ID {id} does not exist");

			//Ánh xạ UpdateRequestModel -> existingUser
			_mapper.Map(userRequest, existingUser);

			//Cập nhật
			existingUser.LastUpdatedTime = Util.GetUtcPlus7Time();

			//Chuyển thành BsonDocument để cập nhật, loại bỏ _id
			BsonDocument bsonDoc = existingUser.ToBsonDocument();
			bsonDoc.Remove("_id");

			//Tạo UpdateDefinition từ BsonDocument
			BsonDocument update = new BsonDocument("$set", bsonDoc);

			UpdateResult result = await _unitOfWork.GetCollection<User>()
				.UpdateOneAsync(filter, update);

			if (result.MatchedCount == 0)
			{
				throw new KeyNotFoundException($"User with ID {id} does not exist");
			}
		}
		#endregion

		#region Cấm tài khoản người dùng (Delete)
		public async Task DeleteByIdAsync(string id)
		{
			User user = await _unitOfWork.GetRepository<User>()
				.Collection
				.Find(u => u.Id == id)
				.FirstOrDefaultAsync()
				?? throw new KeyNotFoundException($"User with ID {id} does not exist");

			if (user.Status == UserStatus.Banned)
			{
				throw new InvalidOperationException("User was previously locked");
			}

			FilterDefinition<User> filter = Builders<User>.Filter.Eq(u => u.Id, id);
			UpdateDefinition<User> update = Builders<User>.Update.Set(u => u.Status, UserStatus.Banned);

			await _unitOfWork.GetRepository<User>().Collection.UpdateOneAsync(filter, update);
		}
		#endregion



	}
}
