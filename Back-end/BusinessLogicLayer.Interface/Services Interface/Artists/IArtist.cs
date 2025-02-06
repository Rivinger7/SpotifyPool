using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Artists
{
    public interface IArtist
    {
        Task CreateArtist(ArtistRequest artistRequest);
        Task<AuthenticatedResponseModel> SwitchToUserProfile();
    }
}
