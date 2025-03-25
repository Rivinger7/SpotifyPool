using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;

namespace BusinessLogicLayer.Interface.Services_Interface.ContentManagers
{
    public interface IContentManager
    {
        Task ChangeTrackRestrictionAsync(string trackId, TrackRestrictionRequestModel model);
    }
}
