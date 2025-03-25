using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Artists
{
    public interface IArtist
    {
        Task CreateArtist(ArtistRequest artistRequest);
        Task<ArtistResponseModel> GetArtistByIdAsync(string artistId);
        Task<IEnumerable<TrackResponseModel>> GetOwnTracks(int offset, int limit);
        Task<IEnumerable<TrackResponseModel>> GetTracksByArtistId(string artistId, int offset, int limit);
        Task<AuthenticatedUserInfoResponseModel> SwitchToUserProfile();
    }
}
