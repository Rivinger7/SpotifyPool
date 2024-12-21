using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack
{
    public class TopTrackResponseModel
    {
        public required string Id { get; set; }

        // Total Tracks
        public required int TotalTracks { get; set; }

        // Top Track Item
        public required IEnumerable<TrackResponseModel> Tracks { get; set; }
    }
}