using BusinessLogicLayer.Interface.Services_Interface.Dashboard;
using BusinessLogicLayer.ModelView.Service_Model_Views.Dashboard.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;

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
			List<Track> topTracks = await _unitOfWork.GetCollection<Track>()
				.Find(FilterDefinition<Track>.Empty)
				.SortByDescending(t => t.StreamCount)
				.Limit(10)
				.ToListAsync();

			List<Artist> topArtists = await _unitOfWork.GetCollection<Artist>()
				.Find(FilterDefinition<Artist>.Empty)
				.SortByDescending(a => a.Followers)
				.Limit(10)
				.ToListAsync();

			List<Track> newTracks = await _unitOfWork.GetCollection<Track>()
				.Find(FilterDefinition<Track>.Empty)
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
					UploadDate = t.UploadDate
				}).ToList(),

				TopArtists = topArtists.Select(a => new Artistes
				{
					Id = a.Id.ToString(),
					Name = a.Name,
					Followers = a.Followers,
					Popularity = a.Popularity
				}).ToList(),

				NewTracks = newTracks.Select(t => new Trackss
				{
					Id = t.Id.ToString(),
					Name = t.Name,
					StreamCount = t.StreamCount,
					Duration = t.Duration,
					UploadDate = t.UploadDate
				}).ToList()
			};

		}
		#endregion

	}
}
