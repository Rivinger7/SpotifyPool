using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Interface.Microservices_Interface.Spotify
{
    public  interface ISpotify 
    {
        Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync();

        string Authorize();
        Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(string authorizationCode);
        Task<string> GetTopTracksAsync(string accessToken, int limit = 2, int offset = 2);
        Task FetchUserSaveTracksAsync(string accessToken, int limit = 2, int offset = 0);
        Task GetAllGenreSeedsAsync(string accessToken);
        Task GetAllMarketsAsync(string accessToken);
        Task<IEnumerable<TrackResponseModel>> TestLookup();
    }
}
