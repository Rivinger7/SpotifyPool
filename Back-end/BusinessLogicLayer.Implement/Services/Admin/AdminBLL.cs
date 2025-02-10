using AutoMapper;
using BusinessLogicLayer.Interface.Services_Interface.Admin;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Services.User;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Admin
{
	public class AdminBLL : IAdmin
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AdminBLL(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		#region Hiển thị thông tin tất cả người dùng (GetPaging)
		public async Task<IEnumerable<AdminResponse>> GetAllAccountAsync(PagingRequestModel request, AdminFilter model)
		{
			List<FilterDefinition<User>> filters = new List<FilterDefinition<User>>();

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

			//Kết hợp all filter
			FilterDefinition<User> combinedFilter = filters.Count > 0
				? Builders<User>.Filter.And(filters)
				: Builders<User>.Filter.Empty;

			IEnumerable<User> result = await _unitOfWork.GetRepository<User>()
				.Collection
				.Find(combinedFilter)
				.SortByDescending(i => i.CreatedTime)  //Sắp xếp theo CreatedTime giảm dần
				.Skip((request.PageNumber - 1) * request.PageSize) //Phân trang
				.Limit(request.PageSize) //Giới hạn số lượng
				.ToListAsync();

			return _mapper.Map<IReadOnlyCollection<AdminResponse>>(result);
		}
		#endregion

		#region Hiển thị đầy đủ thông tin người dùng (GetById)
		public async Task<AdminDetailResponse> GetByIdAsync(string id)
		{
			//Lấy thông tin người dùng theo ID
			User user = await _unitOfWork.GetCollection<User>()
				.Find(user => user.Id == id)
				.FirstOrDefaultAsync();

			//Mapping từ User sang UserResponseModel
			AdminDetailResponse adminResponse = _mapper.Map<AdminDetailResponse>(user);

			return adminResponse;
		}
		#endregion

		#region Chỉnh sửa thông tin người dùng (Update)
		public async Task UpdateByIdAsync(string id, UpdateUserRequest userRequest)
		{
			FilterDefinition<User> filter = Builders<User>.Filter.Eq(u => u.Id, id);

			UpdateDefinition<User> update = Builders<User>.Update
				.Set(u => u.DisplayName, userRequest.DisplayName)
				.Set(u => u.Gender, userRequest.Gender)
				.Set(u => u.PhoneNumber, userRequest.PhoneNumber)
				.Set(u => u.Birthdate, userRequest.Birthdate)
				.Set(u => u.Status, userRequest.Status)
				.Set(u => u.Email, userRequest.Email)
				.Set(u => u.Followers, userRequest.Followers)
				.Set(u => u.Product, userRequest.Product)
				.Set(u => u.Roles, userRequest.Roles)
				.Set(u => u.Images, userRequest.Images)
				.Set(u => u.LastUpdatedTime, Util.GetUtcPlus7Time());

			//Kiểm tra Status
			//if (!string.IsNullOrWhiteSpace(userRequest.Status) && Enum.TryParse(typeof(UserStatus), userRequest.Status, out var status))
			//{
			//	update = update.Set(u => u.Status, (UserStatus)status);
			//}

			UpdateResult result = await _unitOfWork.GetRepository<User>()
				.Collection
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
				.FirstOrDefaultAsync();

			if (user == null)
			{
				throw new KeyNotFoundException($"User with ID {id} does not exist");
			}

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
