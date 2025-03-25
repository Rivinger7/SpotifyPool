using SetupLayer.Enum.Services.Track;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request
{
    public class TrackRestrictionRequestModel
    {
        public RestrictionReason Reason { get; set; }
        public string? ReasonDescription { get; set; }
    }
}
