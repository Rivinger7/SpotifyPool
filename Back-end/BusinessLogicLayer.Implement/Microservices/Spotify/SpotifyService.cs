using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Genres.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Markets.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using DataAccessLayer.Repository.Entities;
using Newtonsoft.Json;
using System.Text.Json;

namespace BusinessLogicLayer.Implement.Microservices.Spotify
{
    public class SpotifyService(IMapper mapper, SpotifyPoolDBContext context) : ISpotify
    {
        private readonly string clientID = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") ?? throw new DataNotFoundCustomException("Client ID property is not set in environment or not found");
        private readonly string clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") ?? throw new DataNotFoundCustomException("Client Secret property is not set in environment or not found");
        private readonly string redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI") ?? throw new DataNotFoundCustomException("Redirect URI property is not set in environment or not found");
        private readonly IMapper _mapper = mapper;
        private readonly SpotifyPoolDBContext _context = context;

        public string Authorize()
        {
            string authorizationEndpoint = "https://accounts.spotify.com/authorize";
            string responseType = "code";
            string scopes = "user-top-read playlist-read-private playlist-modify-public user-library-read";

            // Build the authorization URL with multiple scopes
            string authorizationUrl = $"{authorizationEndpoint}?client_id={clientID}&response_type={responseType}&redirect_uri={redirectUri}&scope={Uri.EscapeDataString(scopes)}";

            return authorizationUrl;
        }

        public async Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(string authorizationCode)
        {
            const string TOKEN_ENDPOINT = "https://accounts.spotify.com/api/token";

            using HttpClient client = new();
            var requestBody = new FormUrlEncodedContent(
            [
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
            ]);

            HttpResponseMessage response = await client.PostAsync(TOKEN_ENDPOINT, requestBody);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            // You should parse the response and return the access token
            JsonDocument tokenResponse = JsonDocument.Parse(responseContent);
            string accessToken = tokenResponse.RootElement.GetProperty("access_token").GetString() ?? throw new DataNotFoundCustomException("Access Token is not found");
            string refreshToken = tokenResponse.RootElement.GetProperty("refresh_token").GetString() ?? throw new DataNotFoundCustomException("Refresh Token is not found");

            return (accessToken, refreshToken);
        }

        public async Task<string> GetTopTracksAsync(string accessToken, int limit = 2, int offset = 2)
        {
            string uri = $"https://api.spotify.com/v1/me/top/tracks?limit={limit}&offset={offset}";

            string responseBody = await GetResponseAsync(uri, accessToken);

            //JsonDocument responseBodyJson = JsonDocument.Parse(responseBody);

            //var topTracks = JsonConvert.DeserializeObject(responseBody);

            return responseBody;
        }

        public async Task<IEnumerable<TrackResponseModel>> GetUserSaveTracksAsync(string accessToken, int limit = 2, int offset = 0)
        {
            string uri = $"https://api.spotify.com/v1/me/tracks?limit={limit}&offset={offset}";

            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize the response body into a list of SpotifyTrack objects
            var spotifyTracks = JsonConvert.DeserializeObject<SpotifyTrack>(responseBody) ?? throw new DataNotFoundCustomException("Not found any tracks");

            // Create a list to store the selected track information
            List<TrackResponseModel> trackModels = [];
            List<string> artistIds = [];

            // Loop through the items and select 'name' and 'preview_url'
            foreach (var item in spotifyTracks.Items)
            {
                // Các phần tử trong TrackDetails được sắp xếp theo thứ tự do dùng loop
                var trackModel = new TrackResponseModel
                {
                    TrackId = item.TrackDetails?.TrackId,
                    Name = item.TrackDetails?.Name,
                    Duration = item.TrackDetails?.Duration,
                    Popularity = item.TrackDetails?.Popularity,
                    PreviewURL = item.TrackDetails?.PreviewUrl,
                    ReleaseDate = item.TrackDetails?.ReleaseDate ?? DateTime.MinValue,
                    Images = item.TrackDetails.AlbumDetails.Images,
                    Artists = item.TrackDetails.Artists
                };

                // Chuyển đổi từng phần tử của AvailableMarkets từ chuỗi thành đối tượng AvailableMarkets
                foreach (var market in item.TrackDetails?.AvailableMarkets)
                {
                    trackModel.AvailableMarkets.Add(new AvailableMarkets { Id = market });
                }

                // Thu thập ID của các nghệ sĩ
                artistIds.AddRange(item.TrackDetails.Artists.Select(a => a.Id));

                trackModels.Add(trackModel);
            }

            // Bước 3: Lọc các artistIds để chỉ giữ lại các ID duy nhất
            var distinctArtistIds = artistIds.Distinct().ToList();

            // Bước 4: Gọi API Spotify để lấy thông tin chi tiết về các nghệ sĩ
            if (distinctArtistIds.Any())
            {
                await SaveArtistsToDatabaseAsync(distinctArtistIds, accessToken);
            }

            // Return the list of track models
            return trackModels;
        }

        private async Task SaveArtistsToDatabaseAsync(List<string> artistIds, string accessToken)
        {
            // Bước 4.1: Tạo URL để gọi API lấy thông tin nghệ sĩ
            string ids = string.Join(",", artistIds);
            await Console.Out.WriteLineAsync($"========================= {ids} ===================");
            string uri = $"https://api.spotify.com/v1/artists?ids={ids}";

            // Bước 4.2: Gọi API và deserializing kết quả
            string responseBody = await GetResponseAsync(uri, accessToken);

            var spotifyArtistsResponse = JsonConvert.DeserializeObject<SpotifyArtist>(responseBody);

            //string responseAsJson = JsonConvert.SerializeObject(spotifyArtistsResponse, Formatting.Indented);

            //await Console.Out.WriteLineAsync($"============= {responseAsJson} ============");

            //Artist a = new()
            //{
            //    SpotifyId = "aaaa",
            //    Name = "bbbb"
            //};

            //await _context.Artists.InsertOneAsync(a);

            List<Artist> artists = [];

            if (spotifyArtistsResponse?.Artists != null)
            {
                foreach (var artistDetails in spotifyArtistsResponse.Artists)
                {
                    //ArtistResponseModel artistResponseModel = new()
                    //{
                    //    SpotifyId = artistDetails.Id,
                    //    Name = artistDetails.Name,
                    //    Followers = artistDetails.Followers.Total,
                    //    Popularity = artistDetails.Popularity,
                    //    Images = artistDetails.Images,
                    //    Genres = artistDetails.Genres,
                    //};

                    Artist artistEntity = new()
                    {
                        SpotifyId = artistDetails.Id,
                        Name = artistDetails.Name,
                        Followers = artistDetails.Followers.Total,
                        Popularity = artistDetails.Popularity,
                        Images = artistDetails.Images,
                        GenreIds = artistDetails.Genres,
                    };

                    artists.Add(artistEntity);

                    //// Chuyển đối tượng thành JSON và in ra console
                    //string artistAsJson = JsonConvert.SerializeObject(artistResponseModel, Formatting.Indented);
                    //await Console.Out.WriteLineAsync($"================ {artistAsJson} ==================");

                    //Sử dụng AutoMapper để ánh xạ từ ArtistResponseModel sang Artist
                    //var artistEntity = _mapper.Map<Artist>(artistResponseModel);

                    // Bước 4.3: Lưu thông tin nghệ sĩ vào cơ sở dữ liệu
                    // Giả sử bạn có một phương thức AddArtistAsync trong repository của bạn
                    //await _context.Artists.InsertOneAsync(artistEntity);
                }
                await _context.Artists.InsertManyAsync(artists);
            }
        }

        public async Task GetAllGenreSeedsAsync(string accessToken)
        {
            string uri = "https://api.spotify.com/v1/recommendations/available-genre-seeds";

            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize the response body into a list of SpotifyTrack objects
            var spotifyGenres = JsonConvert.DeserializeObject<GenreResponseModel>(responseBody) ?? throw new DataNotFoundCustomException("Not found any genres");

            List<Genre> genreList = [];

            foreach(var genreName in spotifyGenres.Name)
            {
                Genre genre = new()
                {
                    Name = genreName
                };

                genreList.Add(genre);
            }

            await _context.Genres.InsertManyAsync(genreList);
        }

        public async Task GetAllMarketsAsync(string accessToken)
        {
            string uri = "\r\nhttps://api.spotify.com/v1/markets";

            string responseBody = await GetResponseAsync(uri, accessToken);

            var spotifyMarkets = JsonConvert.DeserializeObject<MarketResponseModel>(responseBody) ?? throw new DataNotFoundCustomException("Not found any markets");

            List<Market> marketList = [];

            foreach( var marketCode in spotifyMarkets.MarketCode)
            {
                Market market = new()
                {
                    CountryCode = marketCode
                };

                marketList.Add(market);
            }

            await _context.Markets.InsertManyAsync(marketList);
        }

        private static async Task<string> GetResponseAsync(string uri, string accessToken)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            //await Console.Out.WriteLineAsync($"=============== {responseBody} =========================");

            return responseBody;
        }
    }
}
