using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Response
{
	public class TrackInTopTrackResponseModel
	{
		public required string Id { get; set; }
		public required string Name { get; set; }
		public required string PreviewURL { get; set; }
		public required int Duration { get; set; }
		public required string DurationFormated { get; set; }
		public required IEnumerable<ImageResponseModel> Images { get; set; }
	}
}
