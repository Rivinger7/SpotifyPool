using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Genres.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Markets.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Text.Json;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Microservices.Spotify
{
    public class SpotifyService(IMapper mapper, IUnitOfWork unitOfWork) : ISpotify
    {
        private readonly string clientID = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") ?? throw new DataNotFoundCustomException("Client ID property is not set in environment or not found");
        private readonly string clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") ?? throw new DataNotFoundCustomException("Client Secret property is not set in environment or not found");
        private readonly string redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI") ?? throw new DataNotFoundCustomException("Redirect URI property is not set in environment or not found");
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

        public async Task FetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", int limit = 2, int offset = 0)
        {
            // Kiểm tra limit có nhỏ hơn hoặc bằng 0 không
            // Nếu nhỏ hơn hoặc bằng 0 thì không cần fetch
            // Vì Spotify API sẽ trả về lỗi 400 (Bad Request) với message LÀ Invalid Limit
            if (limit <= 0)
            {
                return;
            }

            // Giới hạn số lượng tracks mỗi lần fetch
            int LIMIT_SIZE_MAX = 100;

            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit={Math.Min(limit, LIMIT_SIZE_MAX)}&offset={offset}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyTrack spotifyTracks = JsonConvert.DeserializeObject<SpotifyTrack>(responseBody) ?? throw new DataNotFoundCustomException("Not found any tracks");

            #region Kiểm tra spotifyid có trùng spotifyid trong database không
            // Thu thập TrackId từ Spotify response
            IEnumerable<string?> trackIds = spotifyTracks.Items
                .Where(item => item.TrackDetails?.TrackId is not null)
                .Select(item => item.TrackDetails.TrackId)
                .Distinct()
                .ToList();

            // Truy vấn cơ sở dữ liệu về các track hiện có bằng các SpotifyId cụ thể này
            IEnumerable<string?> existingTrackIds = await _unitOfWork.GetCollection<Track>()
                .Find(Builders<Track>.Filter.In(t => t.SpotifyId, trackIds))
                .Project(t => t.SpotifyId)
                .ToListAsync();
            #endregion

            // Khởi tạo danh sách
            List<string> artistIds = [];
            List<Track> tracks = [];

            // Truy cập các thuộc tính của Response
            foreach (Item item in spotifyTracks.Items)
            {
                // Cách này đã tối ưu hơn vì chỉ cần fetch ra các SpotifyId cần thiết từ list trên
                string trackId = item.TrackDetails?.TrackId;
                if (trackId == null || existingTrackIds.Contains(trackId))
                {
                    continue; // Nếu trùng thì không cần fetch
                }

                // Hàm fetch Audio Features của Track
                string audioFeaturesID = await FetchAudioFeaturesAsync(accessToken, item.TrackDetails?.TrackId);

               // Fetch sang Model
               SpotifyTrackResponseModel trackModel = new()
                {
                    TrackId = item.TrackDetails?.TrackId,
                    Name = item.TrackDetails?.Name,
                    Duration = item.TrackDetails?.Duration,
                    Popularity = item.TrackDetails?.Popularity,
                    PreviewURL = item.TrackDetails?.PreviewUrl,
                    UploadDate = Util.GetUtcPlus7Time().ToString("yyyy-MM-dd"),
                    UploadBy = "Admin",
                    IsExplicit = item.TrackDetails?.IsExplicit,
                    IsPlayable = item.TrackDetails?.IsPlayable,
                    Images = item.TrackDetails.AlbumDetails.Images,
                    Artists = item.TrackDetails.Artists,
                    AudioFeaturesId = audioFeaturesID
                };

                // Ánh xạ từ SpotifyTrackResponseModel sang Track
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

            // Kiểm tra xem danh sách tracks có rỗng không
            if (tracks.Count > 0)
            {
                // Lưu danh sách các Track Entity vào Database
                await _unitOfWork.GetCollection<Track>().InsertManyAsync(tracks);
            }
           
            // Lọc các artistIds để giữ lại các ID duy nhất
            List<string> distinctArtistIds = artistIds.Distinct().ToList();

            // Kiểm tra danh sách có rỗng không
            // Áp dụng thuật toán giống với đệ quy để giới hạn số lượng nghệ sĩ mỗi lần fetch
            if (distinctArtistIds.Count > 0)
            {
                foreach (List<string> artistBatch in Batch(distinctArtistIds, 50))
                {
                    await FetchArtistsByUserSaveTracksAsync(artistBatch, accessToken);
                }
            }

            // Giới hạn limit là 100 tracks mỗi lần fetch
            // Dùng đệ quy để giới hạn limit lại nếu limit vượt quá 100
            // Spotify API sẽ trả về lỗi 400 (Bad Request) với message là Invalid Limit
            // Gọi đệ quy với limit và offset được cập nhật 
            await FetchPlaylistItemsAsync(accessToken, playlistId, (limit - LIMIT_SIZE_MAX), (offset + LIMIT_SIZE_MAX));

            return;
        }

        // Helper method for batching artists' IDs
        private static IEnumerable<List<T>> Batch<T>(List<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
            {
                yield return source.GetRange(i, Math.Min(batchSize, source.Count - i));
            }
        }

        public async Task<string> FetchAudioFeaturesAsync(string accessToken, string? trackId)
        {
            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/audio-features/{trackId}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyAudioFeaturesResponseModel audioFeaturesResponseModel = JsonConvert.DeserializeObject<SpotifyAudioFeaturesResponseModel>(responseBody) ?? throw new DataNotFoundCustomException("Not found any audio features");

            // Truy cập thuộc tính của Response
            AudioFeatures audioFeatures = _mapper.Map<AudioFeatures>(audioFeaturesResponseModel);

            // Lưu danh sách các Audio Feature Entity vào Database
            await _unitOfWork.GetCollection<AudioFeatures>().InsertOneAsync(audioFeatures);

            // Trả về Id của Audio Feature Entity
            return audioFeatures.Id;
        }

        public async Task FetchUserSaveTracksAsync(string accessToken, int limit = 2, int offset = 0)
        {
            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/me/tracks?limit={limit}&offset={offset}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyTrack spotifyTracks = JsonConvert.DeserializeObject<SpotifyTrack>(responseBody) ?? throw new DataNotFoundCustomException("Not found any tracks");

            // Thu thập TrackId từ Spotify response
            IEnumerable<string?> trackIds = spotifyTracks.Items
                .Where(item => item.TrackDetails?.TrackId is not null)
                .Select(item => item.TrackDetails.TrackId)
                .Distinct()
                .ToList();

            // Truy vấn cơ sở dữ liệu về các track hiện có bằng các SpotifyId cụ thể này
            IEnumerable<string?> existingTrackIds = await _unitOfWork.GetCollection<Track>()
                .Find(Builders<Track>.Filter.In(t => t.SpotifyId, trackIds))
                .Project(t => t.SpotifyId)
                .ToListAsync();

            // Khởi tạo danh sách
            List<string> artistIds = [];
            List<Track> tracks = [];

            // Truy cập các thuộc tính của Response
            foreach (Item item in spotifyTracks.Items)
            {
                // Kiểm tra spotifyid có trùng spotifyid trong database không
                // Cách này chưa tối ưu vì cần phải fetch từ database ra hết các SpotifyId
                //IAsyncCursor<Track> existingTrack = await _unitOfWork.GetCollection<Track>().FindAsync(t => t.SpotifyId == (item.TrackDetails != null ? item.TrackDetails.TrackId : null));
                //if (existingTrack.Any())
                //{
                //    continue; // Nếu trùng thì không cần fetch
                //}

                // Kiểm tra spotifyid có trùng spotifyid trong database không
                // Cách này đã tối ưu hơn vì chỉ cần fetch ra các SpotifyId cần thiết từ list trên
                string trackId = item.TrackDetails?.TrackId;
                if (trackId == null || existingTrackIds.Contains(trackId))
                {
                    continue; // Skip if the track ID already exists or is null
                }

                // Fetch sang Model
                var trackModel = new SpotifyTrackResponseModel
                {
                    TrackId = item.TrackDetails?.TrackId,
                    Name = item.TrackDetails?.Name,
                    Duration = item.TrackDetails?.Duration,
                    Popularity = item.TrackDetails?.Popularity,
                    PreviewURL = item.TrackDetails?.PreviewUrl,
                    UploadDate = Util.GetUtcPlus7Time().ToString("yyyy-MM-dd"),
                    UploadBy = "Admin",
                    IsExplicit = item.TrackDetails?.IsExplicit,
                    IsPlayable = item.TrackDetails?.IsPlayable,
                    Images = item.TrackDetails.AlbumDetails.Images,
                    Artists = item.TrackDetails.Artists
                };

                // Chuyển đổi từng phần tử của AvailableMarkets từ chuỗi thành đối tượng AvailableMarkets
                //foreach (string market in item.TrackDetails?.AvailableMarkets)
                //{
                //    trackModel.AvailableMarkets.Add(new AvailableMarkets { Id = market });
                //}

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

            // Nếu đã fetch hết tất cả các track từ Spotify API thì kết thúc luôn
            if (tracks.Count == 0)
            {
                return;
            }

            // Lưu danh sách các Track Entity vào Database
            await _unitOfWork.GetCollection<Track>().InsertManyAsync(tracks);

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

            //Console.WriteLine("===================");
            //Console.WriteLine($"{ids}");
            //Console.WriteLine("===================");

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
                    // Kiểm tra spotifyid có trùng spotifyid trong database không
                    IAsyncCursor<Artist> existingArtist = await _unitOfWork.GetCollection<Artist>().FindAsync(a => a.SpotifyId == artistDetails.Id);
                    if (existingArtist.Any())
                    {
                        continue; // Nếu trùng thì không cần fetch
                    }

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
                await _unitOfWork.GetCollection<Artist>().InsertManyAsync(artists);
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
            await _unitOfWork.GetCollection<Genre>().InsertManyAsync(genreList);
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

            await _unitOfWork.GetCollection<Market>().InsertManyAsync(marketList);
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
    }
}
