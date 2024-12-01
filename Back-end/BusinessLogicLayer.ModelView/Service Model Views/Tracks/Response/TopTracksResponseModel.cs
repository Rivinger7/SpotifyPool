using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response
{
	public class TopTracksResponseModel
	{
		public string TrackId { get; set; }
		public string TrackName { get; set; }
		public string AlbumName { get; set; }
		public int Duration { get; set; }
		public string DurationFormated { get; set; }
		public required IEnumerable<ImageResponseModel> Images { get; set; }
		public required IEnumerable<ArtistResponseModel> Artists { get; set; }
	}
}
