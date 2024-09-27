using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Genres.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Markets.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;
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

        #region Spotify API Server-side
        public string Authorize()
        {
            // Authorize Endpoint chính thức của Spotify
            // OAuth 2.0
            const string AUTHORIZATION_ENDPOINT = "https://accounts.spotify.com/authorize";

            // Authorize Code để trao đổi lấy Access Token và Refresh Token
            const string RESPONSE_TYPE = "code";

            // Các scope để cho phép người yêu cầu truy cập vào các API nhất định của Spotify
            string scopes = "user-top-read playlist-read-private playlist-modify-public user-library-read";

            // Xây dựng URL ủy quyền với nhiều scope
            string authorizationUrl = $"{AUTHORIZATION_ENDPOINT}?client_id={clientID}&response_type={RESPONSE_TYPE}&redirect_uri={redirectUri}&scope={Uri.EscapeDataString(scopes)}";

            return authorizationUrl;
        }

        public async Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(string authorizationCode)
        {
            // Token Endpoint chính thức của Spotify
            const string TOKEN_ENDPOINT = "https://accounts.spotify.com/api/token";

            // Uỷ quyền bằng OAuth 2.0
            using HttpClient client = new();
            FormUrlEncodedContent requestBody = new(
            [
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
            ]);

            // Gọi API để trả về response
            HttpResponseMessage response = await client.PostAsync(TOKEN_ENDPOINT, requestBody);
            response.EnsureSuccessStatusCode();

            // Đọc content của Response
            string responseContent = await response.Content.ReadAsStringAsync();

            // Parse chuỗi Response sang JSON
            JsonDocument tokenResponse = JsonDocument.Parse(responseContent);

            // Lấy AccessToken thông qua thuộc tính access_token và refresh_token
            string accessToken = tokenResponse.RootElement.GetProperty("access_token").GetString() ?? throw new DataNotFoundCustomException("Access Token is not found");
            string refreshToken = tokenResponse.RootElement.GetProperty("refresh_token").GetString() ?? throw new DataNotFoundCustomException("Refresh Token is not found");

            // Trả về cặp token
            return (accessToken, refreshToken);
        }

        public async Task<string> GetTopTracksAsync(string accessToken, int limit = 2, int offset = 2)
        {
            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/me/top/tracks?limit={limit}&offset={offset}";

            // Goi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            return responseBody;
        }

        public async Task FetchUserSaveTracksAsync(string accessToken, int limit = 2, int offset = 0)
        {
            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/me/tracks?limit={limit}&offset={offset}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyTrack spotifyTracks = JsonConvert.DeserializeObject<SpotifyTrack>(responseBody) ?? throw new DataNotFoundCustomException("Not found any tracks");

            // Khởi tạo danh sách
            List<string> artistIds = [];
            List<Track> tracks = [];

            // Truy cập các thuộc tính của Response
            foreach (Item item in spotifyTracks.Items)
            {
                // Fetch sang Model
                var trackModel = new SpotifyTrackResponseModel
                {
                    TrackId = item.TrackDetails?.TrackId,
                    Name = item.TrackDetails?.Name,
                    Duration = item.TrackDetails?.Duration,
                    Popularity = item.TrackDetails?.Popularity,
                    PreviewURL = item.TrackDetails?.PreviewUrl,
                    ReleaseDate = item.TrackDetails?.ReleaseDate,
                    Images = item.TrackDetails.AlbumDetails.Images,
                    Artists = item.TrackDetails.Artists
                };

                // Chuyển đổi từng phần tử của AvailableMarkets từ chuỗi thành đối tượng AvailableMarkets
                foreach (string market in item.TrackDetails?.AvailableMarkets)
                {
                    trackModel.AvailableMarkets.Add(new AvailableMarkets { Id = market });
                }

                // Sử dụng AutoMapper để ánh xạ từ SpotifyTrackResponseModel sang Track
                Track trackEntity = _mapper.Map<Track>(trackModel);

                // Do Images là thuộc tính Array of String
                // Nên sẽ có tới 2 Images Object ở 2 assembly khác nhau
                // Do đó sẽ cần phải map thêm 1 lần nữa với thuộc tính Images
                trackEntity.Images = _mapper.Map<List<Image>>(trackModel.Images); // Có thể thay thế cách này bằng cách map trực tiếp trong Assembly chứa Mapping Class

                // Thêm Track Entity vào danh sách đã khởi tạo
                tracks.Add(trackEntity);

                // Lấy ra ID của các nghệ sĩ
                artistIds.AddRange(item.TrackDetails.Artists.Select(a => a.Id));
            }

            // Lưu danh sách các Track Entity vào Database
            await _context.Tracks.InsertManyAsync(tracks);

            // Lọc các artistIds để giữ lại các ID duy nhất
            List<string> distinctArtistIds = artistIds.Distinct().ToList();

            // Kiểm tra danh sách có rỗng không
            // Không cần thiết
            if (distinctArtistIds.Any())
            {
                // Dùng danh sách Id của các nghệ sĩ để gọi API Spotify để lấy thông tin chi tiết về các nghệ sĩ
                // Đồng thời lưu vào Database
                await FetchArtistsByUserSaveTracksAsync(distinctArtistIds, accessToken);
            }

            return;
        }

        private async Task FetchArtistsByUserSaveTracksAsync(List<string> artistIds, string accessToken)
        {
            // Nối các phần tử trong list bằng ký tự ',' theo định dạng request URI của Spotify
            string ids = string.Join(",", artistIds);

            // URI Several Artists của Spotify
            string uri = $"https://api.spotify.com/v1/artists?ids={ids}";

            // Gọi API để trả về response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyArtist? spotifyArtistsResponse = JsonConvert.DeserializeObject<SpotifyArtist>(responseBody);

            // Khởi tạo danh sách Artist
            List<Artist> artists = [];

            // Kiểm tra Resonse có null không
            // Bước này không cần thiết
            if (spotifyArtistsResponse?.Artists != null)
            {
                // Truy cập vào từng thuộc tính của Response
                // Type của artistDetails là ModelView.Service_Model_Views.Artists.Response
                foreach (var artistDetails in spotifyArtistsResponse.Artists)
                {
                    // Fetch sang Model
                    SpotifyArtistResponseModel artistResponseModel = new()
                    {
                        SpotifyId = artistDetails.Id,
                        Name = artistDetails.Name,
                        Followers = artistDetails.Followers.Total,
                        Popularity = artistDetails.Popularity,
                        Images = artistDetails.Images,
                        Genres = artistDetails.Genres,
                    };

                    // Sử dụng AutoMapper để ánh xạ từ SpotifyArtistResponseModel sang Artist
                    Artist artistEntity = _mapper.Map<Artist>(artistResponseModel);

                    // Do Images là thuộc tính Array of String
                    // Nên sẽ có tới 2 Images Object ở 2 assembly khác nhau
                    // Do đó sẽ cần phải map thêm 1 lần nữa với thuộc tính Images
                    artistEntity.Images = _mapper.Map<List<Image>>(artistResponseModel.Images); // Có thể thay thế cách này bằng cách map trực tiếp trong Assembly chứa Mapping Class

                    // Thêm Artist Entity vào danh sách đã khởi tạo
                    artists.Add(artistEntity);
                }

                // Lưu danh sách các Artist Entity vào Database
                await _context.Artists.InsertManyAsync(artists);
            }
        }

        public async Task GetAllGenreSeedsAsync(string accessToken)
        {
            // URI của Spotify
            string uri = "https://api.spotify.com/v1/recommendations/available-genre-seeds";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            GenreResponseModel spotifyGenres = JsonConvert.DeserializeObject<GenreResponseModel>(responseBody) ?? throw new DataNotFoundCustomException("Not found any genres");

            // Khởi tạo danh sách Genre
            List<Genre> genreList = [];

            // Truy cập thuộc tính Name
            foreach (string genreName in spotifyGenres.Name)
            {
                // Custom Data
                // Thay vì Genre chứa mãng chuỗi thì tạo thành 1 Object riêng biệt
                Genre genre = new()
                {
                    Name = genreName
                };

                // Thêm Genre Entity vào danh sách đã khởi tạo
                genreList.Add(genre);
            }

            // Lưu danh sách các Genre Entity vào Database
            await _context.Genres.InsertManyAsync(genreList);
        }

        // Như trên, lười comment quá
        public async Task GetAllMarketsAsync(string accessToken)
        {
            string uri = "https://api.spotify.com/v1/markets";

            string responseBody = await GetResponseAsync(uri, accessToken);

            MarketResponseModel spotifyMarkets = JsonConvert.DeserializeObject<MarketResponseModel>(responseBody) ?? throw new DataNotFoundCustomException("Not found any markets");

            List<Market> marketList = [];

            foreach (string marketCode in spotifyMarkets.MarketCode)
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
            // Gửi yêu cầu HTTP Header với Bearer tới Spotify App của người dùng
            // Để được ủy quyền thông qua máy chủ khách
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Trả về Response sau khi được ủy quyền thành công
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            // Đọc content của Response
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
        #endregion

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync()
        {
            var tracksWithArtists = await _context.Tracks.Aggregate()
                .Lookup<Track, Artist, TrackWithArtists>(
                    _context.Artists, // The foreign collection
                    track => track.ArtistIds, // The field in Track that we're joining on
                    artist => artist.SpotifyId, // The field in Artist that we're matching against
                    result => result.Artists // The field in TrackWithArtists to hold the matched artists
                )
                .ToListAsync();

            // Map the aggregate result to TrackResponseModel
            var trackModelList = tracksWithArtists.Select(track =>
            {
                var trackResponse = _mapper.Map<TrackResponseModel>(track);
                trackResponse.Artists = _mapper.Map<List<ArtistResponseModel>>(track.Artists);
                return trackResponse;
            });

            return trackModelList;
        }

        // Helper class to hold the aggregate result
        private class TrackWithArtists : Track
        {
            public List<Artist> Artists { get; set; } = [];
        }
    }
}
