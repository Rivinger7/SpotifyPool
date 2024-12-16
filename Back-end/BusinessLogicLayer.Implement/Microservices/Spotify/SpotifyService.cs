using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.Genius;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SetupLayer.Enum.Services.Track;
using SetupLayer.Setting.Microservices.Spotify;
using System.Security.Claims;
using System.Text.Json;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Microservices.Spotify
{
    public class SpotifyService(SpotifySettings spotifySettings, IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IGenius geniusService) : ISpotify
    {
        private readonly string CLIENT_ID = spotifySettings.ClientId;
        private readonly string CLIENT_SECRET = spotifySettings.ClientSecret;
        private readonly string REDIRECT_URI = spotifySettings.RedirectUri;

        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IGenius _geniusService = geniusService;

        #region Spotify API Server-side
        public string Authorize()
        {
            // Authorize Endpoint chính thức của Spotify
            // OAuth 2.0
            const string AUTHORIZATION_ENDPOINT = "https://accounts.spotify.com/authorize";

            // Authorize Code để trao đổi lấy Access Token và Refresh Token
            const string RESPONSE_TYPE = "code";

            // Các scope để cho phép người yêu cầu truy cập vào các API nhất định của Spotify
            const string SCOPES = "user-top-read playlist-read-private playlist-modify-public user-library-read";

            // Xây dựng URL ủy quyền với nhiều scope
            string authorizationUrl = $"{AUTHORIZATION_ENDPOINT}?client_id={CLIENT_ID}&response_type={RESPONSE_TYPE}&redirect_uri={REDIRECT_URI}&scope={Uri.EscapeDataString(SCOPES)}";

            return authorizationUrl;
        }

        public async Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(string authorizationCode)
        {
            // Token Endpoint chính thức của Spotify
            const string TOKEN_ENDPOINT = "https://accounts.spotify.com/api/token";

            // Xây dựng request body để trao đổi lấy Access Token và Refresh Token
            using HttpClient client = new();
            FormUrlEncodedContent requestBody = new(
            [
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("redirect_uri", REDIRECT_URI),
                    new KeyValuePair<string, string>("client_id", CLIENT_ID),
                    new KeyValuePair<string, string>("client_secret", CLIENT_SECRET)
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

        public async Task<string> GetseveralAudioFeaturesAsync(string accessToken, string trackIds)
        {
            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/audio-features?ids={trackIds}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            return responseBody;
        }

        #region Update Fetch Playlist Items
        public async Task UpdateFetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", int? limit = null, int offset = 0)
        {
            // Gọi hàm GetTotalTracksInPlaylist để lấy số lượng tracks(items) trong playlist
            // Nếu limit không được cung cấp thì sẽ lấy tất cả tracks trong playlist
            // Bằng cách này sẽ tránh được việc gọi đệ quy với limit = value not null
            limit ??= await GetTotalTracksInPlaylist(accessToken, playlistId);

            // Giới hạn số lượng tracks mỗi lần fetch
            int LIMIT_SIZE_MAX = 100;

            // Kiểm tra limit có nhỏ hơn hoặc bằng 0 không
            // Nếu nhỏ hơn hoặc bằng 0 thì không cần fetch đồng thời cũng là base case
            // Vì Spotify API sẽ trả về lỗi 400 (Bad Request) với message LÀ Invalid Limit
            if (limit <= 0)
            {
                return;
            }

            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit={Math.Min(limit.Value, LIMIT_SIZE_MAX)}&offset={offset}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyTrack spotifyTracks = JsonConvert.DeserializeObject<SpotifyTrack>(responseBody) ?? throw new DataNotFoundCustomException("Not found any tracksStorage");

            #region Kiểm tra spotifyid có trùng spotifyid trong database không
            // Thu thập SpotifyTrackId từ Spotify response
            IEnumerable<string> spotifyTrackIds = spotifyTracks.Items
                .Select(item => item.TrackDetails.TrackId)
                .Distinct()
                .ToList();

            // Truy vấn cơ sở dữ liệu về các track hiện có bằng các SpotifyId cụ thể này
            IEnumerable<string?> existingTrackIds = await _unitOfWork.GetCollection<Track>()
                .Find(Builders<Track>.Filter.In(track => track.SpotifyId, spotifyTrackIds))
                .Project(t => t.SpotifyId)
                .ToListAsync();
            #endregion

            // Khởi tạo danh sách
            List<string> spotifyArtistIdsStorage = [];
            List<string> artistIdsStorage = [];
            List<Track> tracksStorage = [];
            List<string> audioFeaturesIdsStorage = [];

            // Truy cập các thuộc tính của Response
            #region Truy cập các thuộc tính của Response
            foreach (Item item in spotifyTracks.Items)
            {
                // Cách này đã tối ưu hơn vì chỉ cần fetch ra các SpotifyId cần thiết từ list trên
                if (existingTrackIds.Contains(item.TrackDetails.TrackId))
                {
                    continue; // Nếu trùng thì không cần fetch
                }

                // Tạo mới ObjectId cho Audio Features
                string audioFeaturesID = ObjectId.GenerateNewId().ToString();

                // Lấy ra UserID từ Session
                string userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidDataCustomException("Session timed out. Please login again.");

                // Gán ArtistIds dựa trên ObjectId mới
                //List<string> artistObjectIds = item.TrackDetails.Artists
                //    .Select(artistIdsStorage => ObjectId.GenerateNewId().ToString())
                //    .ToList();

                #region Lấy hoặc tạo mới ArtistIds từ Database
                // Lấy tất cả các SpotifyArtistId của track hiện tại từ response
                List<string> spotifyArtistIds = item.TrackDetails.Artists.Select(spotifyId => spotifyId.Id).ToList();

                // Lấy ra các Artist đã tồn tại trong database bằng cách duyệt qua từng phần tử trong spotifyArtistIds
                IEnumerable<Artist> existingArtists = _unitOfWork.GetCollection<Artist>()
                    .Find(Builders<Artist>.Filter.In(artist => artist.SpotifyId, spotifyArtistIds))
                    .Project(Builders<Artist>.Projection.Include(artist => artist.Id).Include(artist => artist.SpotifyId))
                    .As<Artist>()
                    .ToList();

                // Tạo dictionary để ánh xạ SpotifyId với ObjectId đã có trong database
                Dictionary<string, string> artistIdMap = existingArtists.ToDictionary(artist => artist.SpotifyId, artist => artist.Id);

                // Duyệt qua các nghệ sĩ và lấy ObjectId nếu đã tồn tại, nếu không thì tạo mới
                List<string> artistObjectIds = item.TrackDetails.Artists
                    .Select(artist => artistIdMap.TryGetValue(artist.Id, out var existingId)
                        ? existingId
                        : ObjectId.GenerateNewId().ToString())
                    .ToList();
                #endregion

                // Fetch sang Model
                SpotifyTrackResponseModel trackModel = new()
                {
                    SpotifyTrackId = item.TrackDetails?.TrackId,
                    Name = item.TrackDetails?.Name,
                    Duration = item.TrackDetails?.Duration,
                    Popularity = item.TrackDetails?.Popularity,
                    PreviewURL = item.TrackDetails?.PreviewUrl,
                    UploadDate = Util.GetUtcPlus7Time().ToString("yyyy-MM-dd"),
                    UploadBy = userId,
                    IsExplicit = item.TrackDetails?.IsExplicit,
                    Restrictions = new()
                    {
                        IsPlayable = true,
                        Reason = RestrictionReason.None
                    },
                    Images = item.TrackDetails.AlbumDetails.Images,
                    ArtistIds = artistObjectIds,
                    AudioFeaturesId = audioFeaturesID
                };

                // Ánh xạ từ SpotifyTrackResponseModel sang Track
                Track trackEntity = _mapper.Map<Track>(trackModel);

                // Thêm Track Entity vào danh sách đã khởi tạo
                tracksStorage.Add(trackEntity);

                // Lấy ra ID của các nghệ sĩ
                spotifyArtistIdsStorage.AddRange(spotifyArtistIds);
                artistIdsStorage.AddRange(artistObjectIds);

                // Lấy ra ID của các Audio Features
                audioFeaturesIdsStorage.Add(audioFeaturesID);
            }
            #endregion

            // Kiểm tra xem danh sách tracksStorage có rỗng không
            if (tracksStorage.Count > 0)
            {
                // Lưu danh sách các Track Entity vào Database
                await _unitOfWork.GetCollection<Track>().InsertManyAsync(tracksStorage);

                // Lấy danh sách các Audio Features ID truyền vào hàm FetchAudioFeaturesAsync
                await FetchAudioFeaturesAsync(accessToken, spotifyTrackIds, audioFeaturesIdsStorage);
            }

            // Lọc các spotifyArtistIdsStorage để giữ lại các ID duy nhất
            List<string> distinctSpotifyArtistIds = spotifyArtistIdsStorage.Distinct().ToList();
            List<string> distinctArtistIds = artistIdsStorage.Distinct().ToList();

            // Kiểm tra danh sách có rỗng không
            // Áp dụng thuật toán giống với đệ quy để giới hạn số lượng nghệ sĩ mỗi lần fetch
            if (distinctSpotifyArtistIds.Count > 0)
            {
                foreach (var (batchSpotifyArtistIds, batchArtistIdsObjectId) in Batch(distinctSpotifyArtistIds, distinctArtistIds, 50))
                {
                    await FetchArtistsByTracksAsync(batchSpotifyArtistIds, batchArtistIdsObjectId, accessToken);
                }
            }

            // Giới hạn limit là 100 tracksStorage mỗi lần fetch
            // Dùng đệ quy để giới hạn limit lại nếu limit vượt quá 100
            // Spotify API sẽ trả về lỗi 400 (Bad Request) với message là Invalid Limit
            // Gọi đệ quy với limit và offset được cập nhật 
            await UpdateFetchPlaylistItemsAsync(accessToken, playlistId, (limit - LIMIT_SIZE_MAX), (offset + LIMIT_SIZE_MAX));

            return;
        }
        #endregion

        #region Fetch Playlist Items
        public async Task FetchPlaylistItemsAsync(string accessToken, string playlistId = "5Ezx3uPgLsilYApOpqyujf", Dictionary<string, string>? oldKeyValueArtistPairs = null, int? limit = null, int offset = 0)
        {
            // Sau khi nhận được dictionary từ lần đệ quy trước
            // Mình muốn merge dictionary này với dictionary cũ để tránh việc tạo mới ObjectId nhiều lần
            // Rồi mới tạo mới objectId nếu không tồn tại trong dictionary
            Dictionary<string, string> keyValueArtistPairs = [];
            // Merge dictionary này với dictionary cũ
            // Nếu trùng thì giữ lại ObjectId cũ
            if (oldKeyValueArtistPairs != null)
            {
                keyValueArtistPairs = keyValueArtistPairs.Concat(oldKeyValueArtistPairs).ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            // Gọi hàm GetTotalTracksInPlaylist để lấy số lượng tracksStorage(items) trong playlist
            // Nếu limit không được cung cấp thì sẽ lấy tất cả tracksStorage trong playlist
            // Bằng cách này sẽ tránh được việc gọi đệ quy với limit = value not null
            limit ??= await GetTotalTracksInPlaylist(accessToken, playlistId);

            // Giới hạn số lượng tracks mỗi lần fetch
            int LIMIT_SIZE_MAX = 100;

            // Kiểm tra limit có nhỏ hơn hoặc bằng 0 không
            // Nếu nhỏ hơn hoặc bằng 0 thì không cần fetch đồng thời cũng là base case
            // Vì Spotify API sẽ trả về lỗi 400 (Bad Request) với message LÀ Invalid Limit
            if (limit <= 0)
            {
                return;
            }

            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit={Math.Min(limit.Value, LIMIT_SIZE_MAX)}&offset={offset}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyTrack spotifyTracks = JsonConvert.DeserializeObject<SpotifyTrack>(responseBody) ?? throw new DataNotFoundCustomException("Not found any tracks");

            #region Kiểm tra spotifyid có trùng spotifyid trong database không
            // Thu thập SpotifyTrackId từ Spotify response
            IEnumerable<string> spotifyTrackIds = spotifyTracks.Items
                .Select(item => item.TrackDetails.TrackId)
                .Distinct()
                .ToList();

            // Truy vấn cơ sở dữ liệu về các track hiện có bằng các SpotifyId cụ thể này
            IEnumerable<string?> existingTrackIds = await _unitOfWork.GetCollection<Track>()
                .Find(Builders<Track>.Filter.In(track => track.SpotifyId, spotifyTrackIds))
                .Project(t => t.SpotifyId)
                .ToListAsync();
            #endregion

            // Khởi tạo danh sách
            List<string> spotifyArtistIdsStorage = [];
            List<string> artistIdsStorage = [];
            List<Track> tracksStorage = [];
            List<string> audioFeaturesIdsStorage = [];

            // Lấy ra các spotifyId ngoài vòng lặp trước để khởi tạo objectid
            IEnumerable<string> spotifyArtistIdResponse = spotifyTracks.Items
                .SelectMany(item => item.TrackDetails.Artists.Select(spotifyId => spotifyId.Id))
                .ToList();

            // Duyệt qua từng phần tử trong list trên để lấy ra objectid tương ứng
            // Merge dictionary này với dictionary cũ từ lần đệ quy trước
            keyValueArtistPairs = spotifyArtistIdResponse
                .Distinct()
                .Select(spotifyId => new { SpotifyId = spotifyId, ObjectId = keyValueArtistPairs.TryGetValue(spotifyId, out var existingId) ? existingId : ObjectId.GenerateNewId().ToString() })
                .ToDictionary(spotifyId => spotifyId.SpotifyId, objectId => objectId.ObjectId);
            // Lưu dictionary này vào lần đệ quy tiếp theo để tránh việc tạo mới ObjectId nhiều lần

            // Truy cập các thuộc tính của Response
            #region Truy cập các thuộc tính của Response
            foreach (Item item in spotifyTracks.Items)
            {
                // Cách này đã tối ưu hơn vì chỉ cần fetch ra các SpotifyId cần thiết từ list trên
                if (existingTrackIds.Contains(item.TrackDetails.TrackId))
                {
                    continue; // Nếu trùng thì không cần fetch
                }

                // Tạo mới ObjectId cho Audio Features
                string audioFeaturesID = ObjectId.GenerateNewId().ToString();

                // Lấy ra UserID từ Session
                string userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidDataCustomException("Session timed out. Please login again.");

                #region Lấy hoặc tạo mới ArtistIds
                // Lấy tất cả các SpotifyArtistId của track hiện tại từ response
                IEnumerable<string> spotifyArtistIds = item.TrackDetails.Artists.Select(spotifyId => spotifyId.Id).ToList();

                // Kiểm tra dictionary trên nếu spotifyid trùng item.TrackDetails.Artists.Id thì lấy ra objectId tương ứng
                List<string> artistObjectIds = item.TrackDetails.Artists
                    .Select(artist => keyValueArtistPairs.TryGetValue(artist.Id, out var existingId)
                        ? existingId
                        : ObjectId.GenerateNewId().ToString())
                    .ToList();
                #endregion

                // Fetch sang Model
                SpotifyTrackResponseModel trackModel = new()
                {
                    SpotifyTrackId = item.TrackDetails?.TrackId,
                    Name = item.TrackDetails?.Name,
                    Duration = item.TrackDetails?.Duration,
                    Popularity = item.TrackDetails?.Popularity,
                    PreviewURL = item.TrackDetails?.PreviewUrl,
                    UploadDate = Util.GetUtcPlus7Time().ToString("yyyy-MM-dd"),
                    UploadBy = userId,
                    IsExplicit = item.TrackDetails?.IsExplicit,
                    Restrictions = new()
                    {
                        IsPlayable = true,
                        Reason = RestrictionReason.None
                    },
                    Images = item.TrackDetails.AlbumDetails.Images,
                    ArtistIds = artistObjectIds,
                    AudioFeaturesId = audioFeaturesID
                };

                // Ánh xạ từ SpotifyTrackResponseModel sang Track
                Track trackEntity = _mapper.Map<Track>(trackModel);

                // Thêm Track Entity vào danh sách đã khởi tạo
                tracksStorage.Add(trackEntity);

                // Lấy ra ID của các nghệ sĩ
                spotifyArtistIdsStorage.AddRange(spotifyArtistIds);
                artistIdsStorage.AddRange(artistObjectIds);

                // Lấy ra ID của các Audio Features
                audioFeaturesIdsStorage.Add(audioFeaturesID);
            }
            #endregion

            // Kiểm tra xem danh sách tracksStorage có rỗng không
            if (tracksStorage.Count > 0)
            {
                // Lưu danh sách các Track Entity vào Database
                await _unitOfWork.GetCollection<Track>().InsertManyAsync(tracksStorage);

                // Lấy danh sách các Audio Features ID truyền vào hàm FetchAudioFeaturesAsync
                await FetchAudioFeaturesAsync(accessToken, spotifyTrackIds, audioFeaturesIdsStorage);
            }

            // Lọc các 

            // Lọc các spotifyArtistIdsStorage để giữ lại các ID duy nhất
            List<string> distinctSpotifyArtistIds = spotifyArtistIdsStorage.Distinct().ToList();
            List<string> distinctArtistIds = artistIdsStorage.Distinct().ToList();

            // Kiểm tra danh sách có rỗng không
            // Áp dụng thuật toán giống với đệ quy để giới hạn số lượng nghệ sĩ mỗi lần fetch
            if (distinctSpotifyArtistIds.Count > 0)
            {
                foreach (var (batchSpotifyArtistIds, batchArtistIdsObjectId) in Batch(distinctSpotifyArtistIds, distinctArtistIds, 50))
                {
                    await FetchArtistsByTracksAsync(batchSpotifyArtistIds, batchArtistIdsObjectId, accessToken);
                }
            }

            // Giới hạn limit là 100 tracks mỗi lần fetch
            // Dùng đệ quy để giới hạn limit lại nếu limit vượt quá 100
            // Spotify API sẽ trả về lỗi 400 (Bad Request) với message là Invalid Limit
            // Gọi đệ quy với limit và offset được cập nhật 
            await FetchPlaylistItemsAsync(accessToken, playlistId, keyValueArtistPairs, (limit - LIMIT_SIZE_MAX), (offset + LIMIT_SIZE_MAX));

            return;
        }
        #endregion
        // Helper method for getting total tracks in a playlist
        private static async Task<int> GetTotalTracksInPlaylist(string accessToken, string playlistId)
        {
            string TRACKS_TOTAL = "tracks.total";

            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/playlists/{playlistId}?fields={TRACKS_TOTAL}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Parse chuỗi Response sang JSON
            JsonDocument responseJson = JsonDocument.Parse(responseBody);
            int totalTracks = responseJson.RootElement.GetProperty("tracks").GetProperty("total").GetInt32();

            // Trả về số lượng tracks trong playlist
            return totalTracks;
        }

        // Helper method for batching artists' IDs
        private static IEnumerable<(IEnumerable<T1>, IEnumerable<T2>)> Batch<T1, T2>(List<T1> spotifyArtistIds, List<T2> artistIdsObjectId, int batchSize)
        {
            // Chỉ batch theo `spotifyArtistIdsStorage` (Spotify IDs)
            for (int i = 0; i < spotifyArtistIds.Count; i += batchSize)
            {
                IEnumerable<T1> batchSpotifyArtistIds = spotifyArtistIds.GetRange(i, Math.Min(batchSize, spotifyArtistIds.Count - i));
                IEnumerable<T2> batchArtistIdsObjectId = artistIdsObjectId.GetRange(i, Math.Min(batchSize, artistIdsObjectId.Count - i));

                yield return (batchSpotifyArtistIds, batchArtistIdsObjectId);
            }
        }

        private async Task FetchAudioFeaturesAsync(string accessToken, IEnumerable<string> spotifyTrackIds, IEnumerable<string> trackObjectIds)
        {
            // Nối các phần tử trong list bằng ký tự ',' theo định dạng request URI của Spotify
            string ids = string.Join(",", spotifyTrackIds);

            // URI của Spotify
            string uri = $"https://api.spotify.com/v1/audio-features?ids={ids}";

            // Gọi API trả về Response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyAudioFeatures audioFeaturesResponseModel = JsonConvert.DeserializeObject<SpotifyAudioFeatures>(responseBody) ?? throw new DataNotFoundCustomException("Not found any audio features");

            // Khởi tạo danh sách Audio Features
            List<AudioFeatures> audioFeatures = [];

            // AudioFeatures Response Model
            foreach ((AudioFeaturesResponse audioFeaturesItem, string objectId) in audioFeaturesResponseModel.AudioFeatures.Zip(trackObjectIds))
            {
                SpotifyAudioFeaturesResponseModel spotifyAudioFeaturesResponseModel = new()
                {
                    Id = objectId,
                    Danceability = audioFeaturesItem.Danceability,
                    Energy = audioFeaturesItem.Energy,
                    Key = audioFeaturesItem.Key,
                    Loudness = audioFeaturesItem.Loudness,
                    Mode = audioFeaturesItem.Mode,
                    Speechiness = audioFeaturesItem.Speechiness,
                    Acousticness = audioFeaturesItem.Acousticness,
                    Instrumentalness = audioFeaturesItem.Instrumentalness,
                    Liveness = audioFeaturesItem.Liveness,
                    Valence = audioFeaturesItem.Valence,
                    Tempo = audioFeaturesItem.Tempo,
                    Duration = audioFeaturesItem.Duration,
                    TimeSignature = audioFeaturesItem.TimeSignature
                };

                // Ánh xạ từ SpotifyAudioFeaturesResponseModel sang AudioFeatures
                AudioFeatures audioFeaturesEntity = _mapper.Map<AudioFeatures>(spotifyAudioFeaturesResponseModel);

                // Thêm Audio Feature Entity vào danh sách đã khởi tạo
                audioFeatures.Add(audioFeaturesEntity);
            }

            if(audioFeatures.Count > 0)
            {
                // Lưu danh sách các Audio Feature Entity vào Database
                await _unitOfWork.GetCollection<AudioFeatures>().InsertManyAsync(audioFeatures);
            }
        }

        private async Task FetchArtistsByTracksAsync(IEnumerable<string> spotifyArtistIds, IEnumerable<string> artistObjectIds, string accessToken)
        {
            // Nối các phần tử trong list bằng ký tự ',' theo định dạng request URI của Spotify
            string ids = string.Join(",", spotifyArtistIds);

            // URI Several ArtistIds của Spotify
            string uri = $"https://api.spotify.com/v1/artists?ids={ids}";

            // Gọi API để trả về response
            string responseBody = await GetResponseAsync(uri, accessToken);

            // Deserialize Object theo Type là Response/Request Model
            SpotifyArtist spotifyArtistsResponse = JsonConvert.DeserializeObject<SpotifyArtist>(responseBody)
                ?? throw new InvalidDataCustomException("Can not deserialize SpotifyArtist from response body because of null value.");

            #region Kiểm tra spotifyid có trùng spotifyid trong database không
            // Thu thập SpotifyArtistId từ Spotify response
            IEnumerable<string?> spotifyArtistIdsResponse = spotifyArtistsResponse.Artists
                .Select(item => item.Id)
                .ToList();

            // Truy vấn cơ sở dữ liệu về các track hiện có bằng các SpotifyId cụ thể này
            List<string?> existingSpotifyArtistIds = await _unitOfWork.GetCollection<Artist>()
                .Find(Builders<Artist>.Filter.In(artist => artist.SpotifyId, spotifyArtistIdsResponse))
                .Project(t => t.SpotifyId)
                .ToListAsync();

            // Kiểm tra Id đã tồn tại trong database chưa
            IEnumerable<string?> existingArtistObjectIds = await _unitOfWork.GetCollection<Artist>()
                .Find(Builders<Artist>.Filter.In(artist => artist.Id, artistObjectIds))
                .Project(t => t.Id)
                .ToListAsync();
            #endregion

            // Khởi tạo danh sách Artist
            List<Artist> artists = [];

            // Truy cập vào từng thuộc tính của Response
            // Type của artistDetails là ModelView.Service_Model_Views.ArtistIds.Response
            foreach (var (artistDetails, objectId) in spotifyArtistsResponse.Artists.Zip(artistObjectIds))
            {
                //// Kiểm tra spotifyid có trùng spotifyid trong database không
                if (existingSpotifyArtistIds.Contains(artistDetails.Id) || existingArtistObjectIds.Contains(objectId))
                {
                    continue;
                }

                // Fetch sang Model
                SpotifyArtistResponseModel artistResponseModel = new()
                {
                    Id = objectId,
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
                //artistEntity.Images = _mapper.Map<List<Image>>(artistResponseModel.Images); // Có thể thay thế cách này bằng cách map trực tiếp trong Assembly chứa Mapping Class

                // Thêm Artist Entity vào danh sách đã khởi tạo
                artists.Add(artistEntity);
            }

            // Kiểm tra xem danh sách có rỗng không
            if (artists.Count > 0)
            {
                // Lưu danh sách các Artist Entity vào Database
                await _unitOfWork.GetCollection<Artist>().InsertManyAsync(artists);
            }
        }

        public async Task FetchLyricsAsync(string accessToken)
        {
            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Name)
                .Include(track => track.Artists);

            // Empty Pipeline  
            IAggregateFluent<Track> pipeLine = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lookup  
            IAggregateFluent<ASTrack> trackPipelines = pipeLine.Lookup<Track, Artist, ASTrack>
                (_unitOfWork.GetCollection<Artist>(), // The foreign collection  
                track => track.ArtistIds, // The field in Track that are joining on  
                artist => artist.Id, // The field in Artist that are matching against  
                result => result.Artists) // The field in ASTrack to hold the matched artists  
                .Project(projectionDefinition)
                .As<ASTrack>();

            // Pipeline to list  
            IEnumerable<ASTrack> tracks = await trackPipelines.ToListAsync();

            // Map the aggregate result to TrackResponseModel  
            IEnumerable<TrackResponseModel> responseModel = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            // Fetch Lyrics
            foreach (TrackResponseModel track in responseModel)
            {
                string? lyrics = await _geniusService.GetUrlLyricsAsync(accessToken, track.Name, track.Artists.Select(artist => artist.Name).First());

                if (lyrics != null)
                {
                    UpdateDefinition<Track> update = Builders<Track>.Update.Set(t => t.Lyrics, lyrics);
                    await _unitOfWork.GetCollection<Track>().UpdateOneAsync(t => t.Id == track.Id, update);
                }
            }
        }

        private static async Task<string> GetResponseAsync(string uri, string accessToken, int maxRetries = 10)
        {
            // Gửi yêu cầu HTTP Header với Bearer tới Spotify App của người dùng
            // Để được ủy quyền thông qua máy chủ khách
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            //// Trả về Response sau khi được ủy quyền thành công
            //HttpResponseMessage response = await client.GetAsync(uri);
            //response.EnsureSuccessStatusCode();

            //// Đọc content của Response
            //string responseBody = await response.Content.ReadAsStringAsync();

            //return responseBody;

            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                // Gửi yêu cầu HTTP tới Spotify
                HttpResponseMessage response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    // Nếu thành công, đọc và trả về nội dung response
                    return await response.Content.ReadAsStringAsync();
                }

                // Kiểm tra nếu bị giới hạn bởi lỗi 429 Too Many Requests
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    // Đọc giá trị từ header `Retry-After`
                    if (response.Headers.TryGetValues("Retry-After", out var values))
                    {
                        int retryAfter = int.Parse(values.First());
                        Console.WriteLine($"Rate limit exceeded. Retrying after {retryAfter} seconds...");

                        // Đợi số giây theo `Retry-After` trước khi thử lại
                        await Task.Delay(retryAfter * 1000);
                    }
                    else
                    {
                        // Nếu không có header `Retry-After`, sử dụng backoff mặc định
                        int defaultBackoff = (retryCount + 1) * 1000;
                        Console.WriteLine($"Rate limit exceeded. Retrying after {defaultBackoff / 1000} seconds...");
                        await Task.Delay(defaultBackoff);
                    }

                    retryCount++;
                }
                else
                {
                    // Nếu lỗi không phải 429, ném ra ngoại lệ
                    response.EnsureSuccessStatusCode();
                }
            }

            throw new HttpRequestException($"Failed to get a response from Spotify after {maxRetries} attempts.");
        }
        #endregion
    }
}
