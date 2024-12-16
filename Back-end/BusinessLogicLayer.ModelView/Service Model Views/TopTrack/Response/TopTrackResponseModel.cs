using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack
{
    public class TopTrackResponseModel
    {
        public string TopTrackId { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public List<TracksInfoResponse> TrackInfo { get; set; } = [];
    }

    public class TracksInfoResponse
    {
        public string TrackId { get; set; } = null!;

        public int StreamCount { get; set; }

        public DateTime FirstAccessTime { get; set; } = Utility.Coding.Util.GetUtcPlus7Time();

        public TrackInTopTrackResponseModel? Track { get; set; }

        public List<string>? Artists { get; set; }
    }
}