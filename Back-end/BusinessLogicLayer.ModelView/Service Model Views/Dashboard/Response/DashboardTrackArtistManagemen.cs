using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Dashboard.Response
{
	public class DashboardTrackArtistManagemen
	{
		public List<Trackss> TopTracks { get; set; } = new List<Trackss>();
		public List<Artistes> TopArtists { get; set; } = new List<Artistes>();
		public List<Trackss> NewTracks { get; set; } = new List<Trackss>();
	}

	public class Trackss
	{
		public string Id { get; set; } = null!;
		public string Name { get; set; } = null!;
		public long StreamCount { get; set; }
		public int Duration { get; set; }
		public string UploadDate { get; set; }
		public List<Image> Images { get; set; } = [];
	}

	public class Artistes
	{
		public string Id { get; set; } = null!;
		public string Name { get; set; } = null!;
		public int Followers { get; set; }
		public int Popularity { get; set; }
		public List<Image> Images { get; set; } = [];
	}
}
