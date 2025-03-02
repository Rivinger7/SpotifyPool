using SetupLayer.Enum.Services.Track;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request
{
	public class TrackFilterModel
	{
		public string? SearchTerm { get; set; }

		public bool? SortById { get; set; }

		public bool? SortByName { get; set; }

		public Mood? Mood { get; set; }
    }
}
