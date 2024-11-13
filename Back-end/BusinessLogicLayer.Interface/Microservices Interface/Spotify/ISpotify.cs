using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Interface.Microservices_Interface.Spotify
{
    public  interface ISpotify 
    {
        string Authorize();
        Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(string authorizationCode);
        Task<string> GetTopTracksAsync(string accessToken, int limit = 2, int offset = 2);
        Task FetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", Dictionary<string, string>? oldKeyValueArtistPairs = null, int? limit = null, int offset = 0);
        Task UpdateFetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", int? limit = null, int offset = 0);
        Task FetchLyricsAsync(string accessToken);
    }
}
