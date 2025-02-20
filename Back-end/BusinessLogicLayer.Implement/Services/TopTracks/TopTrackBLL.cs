using BusinessLogicLayer.Interface.Services_Interface.TopTracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using StackExchange.Redis;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.TopTracks
{
    public class TopTrackBLL(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IConnectionMultiplexer redis) : ITopTrack
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDatabase _redis = redis.GetDatabase();

        public async Task UpsertTopTrackAsync(TopTrackRequestModel topTrackRequestModel)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Lấy thông tin topTrack của user
            TopTrack topTrack = await _unitOfWork.GetCollection<TopTrack>()
                .Find(topTrack => topTrack.UserId == userID)
                //.Project(topTrack => topTrack.Id)
                .FirstOrDefaultAsync();

            // Nếu không có topTrack thì tạo mới
            if (topTrack is null)
            {
                TopTrack newTopTrack = new()
                {
                    UserId = userID,
                    TrackInfo =
                    [
                        new()
                        {
                            TrackId = topTrackRequestModel.TrackId,
                            StreamCount = 1
                        }
                    ]
                };

                // Lưu thông tin topTrack mới
                await _unitOfWork.GetCollection<TopTrack>().InsertOneAsync(newTopTrack);

                return;
            }

            // Nếu có topTrack thì cập nhật
            // Kiểm tra xem track đã tồn tại trong topTrack chưa
            TopTrackInfo? trackInfo = topTrack.TrackInfo.Find(track => track.TrackId == topTrackRequestModel.TrackId);

            // Nếu chưa tồn tại thì thêm mới topTrackInfo
            if (trackInfo is null)
            {
                topTrack.TrackInfo.Add(new()
                {
                    TrackId = topTrackRequestModel.TrackId,
                    StreamCount = 1
                });

                // Lưu thông tin topTrack cập nhật
                // Last là thông tin vừa được thêm vào như trên
                UpdateDefinition<TopTrack> addToSetUpdateDefinition = Builders<TopTrack>.Update.AddToSet(topTrack => topTrack.TrackInfo, topTrack.TrackInfo.Last());
                await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(topTrack => topTrack.UserId == userID, addToSetUpdateDefinition);

                return;
            }

            // Nếu đã tồn tại thì cập nhật Stream Count của topTrackInfo
            // Filter topTrackInfo cần cập nhật
            FilterDefinition<TopTrack> filterDefinition = Builders<TopTrack>.Filter.And(
                Builders<TopTrack>.Filter.Eq(topTrack => topTrack.UserId, userID),
                Builders<TopTrack>.Filter.ElemMatch(topTrack => topTrack.TrackInfo, track => track.TrackId == topTrackRequestModel.TrackId));

            // Cập nhật Stream Count của topTrackInfo
            // Do MongoDB không hỗ trợ hoàn toàn các Linq query nên phải thao tác trực tiếp với MongoDB
            UpdateDefinition<TopTrack> updateDefinition = Builders<TopTrack>.Update.Inc(topTrack => topTrack.TrackInfo.FirstMatchingElement().StreamCount, 1);
            await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(filterDefinition, updateDefinition);

            return;
        }

        public async Task<IEnumerable<TrackResponseModel>> GetTopTrackAsync()
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Lấy trackId từ topTrack của user và sắp xếp theo StreamCount
            IEnumerable<string> trackIds = await _unitOfWork.GetCollection<TopTrack>()
                .Find(topTrack => topTrack.UserId == userID)
                .Project(topTrack => topTrack.TrackInfo
                    .OrderByDescending(trackInfo => trackInfo.StreamCount)
                    .Select(trackInfo => trackInfo.TrackId))
                .FirstOrDefaultAsync();

            // Filter
            FilterDefinition<Track> filterDefinition = Builders<Track>.Filter.In(track => track.Id, trackIds);

            // Sắp xếp theo Stream Count của Top Track
            //SortDefinition<Track> sortDefinition = Builders<Track>.Sort.Descending(track => track.Popularity);

            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> tracksInTopTrackProjection = Builders<ASTrack>.Projection.Expression(track => new TrackResponseModel
            {
                Id = track.Id,
                Name = track.Name,
                Description = track.Description,
                Lyrics = track.Lyrics,
                PreviewURL = track.StreamingUrl,
                Duration = track.Duration,
                Images = track.Images.Select(image => new ImageResponseModel
                {
                    URL = image.URL,
                    Height = image.Height,
                    Width = image.Width
                }),
                Artists = track.Artists.Select(artist => new ArtistResponseModel
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Followers = artist.Followers,
                    GenreIds = artist.GenreIds,
                    Images = artist.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    })
                })
            });

            // Stage
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Match(filterDefinition)
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project(tracksInTopTrackProjection)
                .ToListAsync();

            // Maintain the order of tracks based on the sorted trackIds
            tracksResponseModel = trackIds
                .Select(trackId => tracksResponseModel.First(track => track.Id == trackId))
                .ToList();

            return tracksResponseModel;
        }

        public async Task UpdateStreamCountAsync(string trackId)
        {
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");      
            }

            string key = $"stream_count:{userID}";
            await _redis.HashIncrementAsync(key, trackId, 1);
            return;
        }
    }
}
