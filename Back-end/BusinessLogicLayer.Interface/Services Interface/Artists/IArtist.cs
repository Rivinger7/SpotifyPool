using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Artists
{
    public interface IArtist
    {
        Task CreateArtist(ArtistRequest artistRequest);
        Task<IEnumerable<TrackResponseModel>> GetOwnTracks(int offset, int limit);
        Task<AuthenticatedUserInfoResponseModel> SwitchToUserProfile();
    }
}
