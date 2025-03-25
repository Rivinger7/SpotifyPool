using BusinessLogicLayer.Interface.Services_Interface.Dashboard;
using BusinessLogicLayer.ModelView.Service_Model_Views.Dashboard.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;
using SetupLayer.Enum.Services.User;
using System.Globalization;
using System.Linq;

namespace BusinessLogicLayer.Implement.Services.Dashboard
{
	public class DashboardBLL : IDashboard
	{
		private readonly IUnitOfWork _unitOfWork;

		public DashboardBLL(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		#region Thống kê tổng quan hệ thống
		public async Task<SystemOverviewResponse> GetSystemOverviewAsync()
		{
			long totalUsers = await _unitOfWork.GetCollection<User>().CountDocumentsAsync(FilterDefinition<User>.Empty);
			long totalArtists = await _unitOfWork.GetCollection<Artist>().CountDocumentsAsync(FilterDefinition<Artist>.Empty);
			long totalTracks = await _unitOfWork.GetCollection<Track>().CountDocumentsAsync(FilterDefinition<Track>.Empty);
			long totalAlbums = await _unitOfWork.GetCollection<Album>().CountDocumentsAsync(FilterDefinition<Album>.Empty);
			long totalPlaylists = await _unitOfWork.GetCollection<Playlist>().CountDocumentsAsync(FilterDefinition<Playlist>.Empty);

			return new SystemOverviewResponse
			{
				TotalUsers = totalUsers,
				TotalArtists = totalArtists,
				TotalTracks = totalTracks,
				TotalAlbums = totalAlbums,
				TotalPlaylists = totalPlaylists,
			};
		}
		#endregion

		#region Quản lý bài hát & nghệ sĩ (Top bài hát, Top nghệ sĩ)
		public async Task<DashboardTrackArtistManagemen> GetTrackArtistManagementAsync()
		{
			//Top 10 bài hát có lượt stream cao nhất
			List<Track> topTracks = await _unitOfWork.GetCollection<Track>()
				.Find(FilterDefinition<Track>.Empty)
				.SortByDescending(t => t.StreamCount)
				.Limit(10)
				.ToListAsync();

			//Top 10 nghệ sĩ có nhiều follower nhất
			List<Artist> topArtists = await _unitOfWork.GetCollection<Artist>()
				.Find(FilterDefinition<Artist>.Empty)
				.SortByDescending(a => a.Followers)
				.Limit(10)
				.ToListAsync();

			//10 Bài hát mới nhất
			List<Track> newTracks = await _unitOfWork.GetCollection<Track>()
				.Find(FilterDefinition<Track>.Empty)
				.SortByDescending(t => t.UploadDate)
				.Limit(10)
				.ToListAsync();

			return new DashboardTrackArtistManagemen
			{
				TopTracks = topTracks.Select(t => new Trackss
				{
					Id = t.Id.ToString(),
					Name = t.Name,
					StreamCount = t.StreamCount,
					Duration = t.Duration,
					UploadDate = t.UploadDate,
					Images = t.Images,
				}).ToList(),

				TopArtists = topArtists.Select(a => new Artistes
				{
					Id = a.Id.ToString(),
					Name = a.Name,
					Followers = a.Followers,
					Popularity = a.Popularity,
					Images = a.Images
				}).ToList(),

				NewTracks = newTracks.Select(t => new Trackss
				{
					Id = t.Id.ToString(),
					Name = t.Name,
					StreamCount = t.StreamCount,
					Duration = t.Duration,
					UploadDate = t.UploadDate,
					Images = t.Images
				}).ToList()
			};

		}
		#endregion

		#region Tổng quan: so sánh giữa người dùng mới với người dùng trong tháng đó
		public async Task<List<UserGrowthDashboard>> GetUserGrowthAsync()
		{
			//Tạo ngày bắt đầu từ 1/1 năm hiện tại
			DateTime startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);

			//Lấy all user theo CreatTime/LastLoginTime nằm trong hiện tại
			List<User> users = await _unitOfWork.GetCollection<User>()
				.Find(u => u.CreatedTime >= startOfYear || u.LastLoginTime >= startOfYear)
				.ToListAsync();

			//Duyệt qua từng tháng (1 đến 12), thống kê số lượng user mới & user active theo tháng
			var months = Enumerable.Range(1, 12).Select(m => new
			{
				//Tháng hiện tại trong vòng lặp (1 -> 12)
				Month = m,

				//Số USer được đăng ký trong tháng m
				NewUsers = users.Count(u => u.CreatedTime.Month == m),

				//Số User hoạt động trong tháng m
				ActiveUsers = users.Count(u => u.LastLoginTime.HasValue && u.LastLoginTime.Value.Month == m)
			});


			return months.Select(m => new UserGrowthDashboard
			{
				//Chuyển số tháng (1) => "Jan"
				Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Month),
				NewUsers = m.NewUsers,
				ActiveUsers = m.ActiveUsers
			}).ToList();
		}
		#endregion

		#region Tỷ lệ người dùng theo role
		public async Task<List<RoleDistributionDashboard>> GetUserRoleDistributionAsync()
		{
			//Lấy toàn bộ User
			List<User> users = await _unitOfWork.GetCollection<User>()
				.Find(FilterDefinition<User>.Empty)
				.ToListAsync();

			int totalUsers = users.Count;

			//Ưu tiên: Admin > Artist > Customer > ...
			var rolePriority = new[] { UserRole.Admin, UserRole.Artist, UserRole.Customer, UserRole.ContentManager };

			List<RoleDistributionDashboard> mainRoles = users
				.Select(user =>
				{
					UserRole main = user.Roles.FirstOrDefault(r => rolePriority.Contains(r));
					return new { UserId = user.Id, MainRole = main };
				})
				.Where(x => x.MainRole != null)
				.GroupBy(x => x.MainRole)
				.Select(group => new RoleDistributionDashboard
				{
					Role = group.Key!.ToString(),	//Tên role (Customer, Artist,...)
					Count = group.Count(),		//Tổng User có Role này
					Percentage = Math.Round((double)group.Count() * 100 / totalUsers, 2)
				})
				.OrderByDescending(x => x.Count)
				.ToList();

			return mainRoles;
		}

		#endregion

	}
}
