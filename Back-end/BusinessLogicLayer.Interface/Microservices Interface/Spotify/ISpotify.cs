using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Interface.Microservices_Interface.Spotify
{
    public  interface ISpotify 
    {
        string Authorize();
        Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(string authorizationCode);
        Task<string> GetTopTracksAsync(string accessToken, int limit = 2, int offset = 2);
        Task<string> GetseveralAudioFeaturesAsync(string accessToken, string trackIds);
        Task FetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", Dictionary<string, string>? oldKeyValueArtistPairs = null, int? limit = null, int offset = 0);
        Task UpdateFetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", int? limit = null, int offset = 0);
        Task FetchLyricsAsync(string accessToken);
        Task<(List<Image> trackImages, Dictionary<string, string> artistDictionary, Dictionary<string, List<Image>> artistImages, Dictionary<string, int> artistPpularity, Dictionary<string, int> artistFollower)> FetchTrackAsync(string accessToken, string trackId);
        //Task FixTracksWithArtistIsNullAsync(string accessToken);
    }
}
