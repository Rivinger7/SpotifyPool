using SetupLayer.Enum.Services.Track;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Restrictions.Response
{
    public class RestrictionsResponseModel
    {
        public required bool IsPlayable { get; set; }
        public required RestrictionReason Reason { get; set; }
    }
}
