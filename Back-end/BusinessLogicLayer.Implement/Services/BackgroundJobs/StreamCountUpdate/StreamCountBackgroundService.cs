using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using StackExchange.Redis;

namespace BusinessLogicLayer.Implement.Services.BackgroundJobs.StreamCountUpdate
{
    public class StreamCountBackgroundService(IServiceScopeFactory factory, IConnectionMultiplexer redis) : BackgroundService
    {
        private readonly IServiceScopeFactory _factory = factory;
        private readonly IDatabase _redis = redis.GetDatabase();
        private readonly IServer server = redis.GetServer(redis.GetEndPoints().First());

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _factory.CreateScope())
                {
                    IUnitOfWork _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    {
                        var tran = _redis.CreateTransaction();

                        foreach (var key in server.Keys(pattern: "stream_count:*"))
                        {
                            await UpdateCountToMongoDB(_unitOfWork, key);
                            await _redis.KeyDeleteAsync(key);
                        }

                        bool committed = await tran.ExecuteAsync();

                        if (committed)
                        {
                            Console.WriteLine("Stream count updated to MongoDB");
                        }

                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                }
            }
        }

        private async Task UpdateCountToMongoDB(IUnitOfWork _unitOfWork, RedisKey key)
        {

            // Lấy thông tin user từ Redis
            string[] keyParts = key.ToString().Split(':');
            string userId = keyParts[1];

            var hashEntry = await _redis.HashGetAllAsync(key);

            foreach (var entry in hashEntry)
            {
                string trackId = entry.Name;
                // Lấy stream count từ Redis
                int playedTrackCount = (int)entry.Value;

                if (playedTrackCount > 0)
                {
                    // Lấy thông tin topTrack của user
                    TopTrack topTrack = await _unitOfWork.GetCollection<TopTrack>()
                        .Find(topTrack => topTrack.UserId == userId)
                        //.Project(topTrack => topTrack.Id)
                        .FirstOrDefaultAsync();

                    // Nếu không có topTrack thì tạo mới
                    if (topTrack is null)
                    {
                        TopTrack newTopTrack = new()
                        {
                            UserId = userId,
                            CreatedTime = DateTime.UtcNow,
                            TrackInfo =
                            [
                                new()
                            {
                                TrackId = trackId,
                                StreamCount = playedTrackCount
                            }
                            ]
                        };

                        // Lưu thông tin topTrack mới
                        await _unitOfWork.GetCollection<TopTrack>().InsertOneAsync(newTopTrack);

                        return;
                    }

                    // Nếu có topTrack thì cập nhật
                    // Kiểm tra xem track đã tồn tại trong topTrack chưa
                    TopTrackInfo? trackInfo = topTrack.TrackInfo.Find(track => track.TrackId == trackId);

                    // Nếu chưa tồn tại thì thêm mới topTrackInfo
                    if (trackInfo is null)
                    {
                        topTrack.TrackInfo.Add(new()
                        {
                            TrackId = trackId,
                            StreamCount = playedTrackCount
                        });

                        // Lưu thông tin topTrack cập nhật
                        // Last là thông tin vừa được thêm vào như trên
                        UpdateDefinition<TopTrack> addToSetUpdateDefinition = Builders<TopTrack>.Update.AddToSet(topTrack => topTrack.TrackInfo, topTrack.TrackInfo.Last());
                        await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(topTrack => topTrack.UserId == userId, addToSetUpdateDefinition);

                        return;
                    }

                    // Nếu đã tồn tại thì cập nhật Stream Count của topTrackInfo
                    // Filter topTrackInfo cần cập nhật
                    FilterDefinition<TopTrack> filterDefinition = Builders<TopTrack>.Filter.And(
                        Builders<TopTrack>.Filter.Eq(topTrack => topTrack.UserId, userId),
                        Builders<TopTrack>.Filter.ElemMatch(topTrack => topTrack.TrackInfo, track => track.TrackId == trackId));

                    // Cập nhật Stream Count của topTrackInfo
                    // Do MongoDB không hỗ trợ hoàn toàn các Linq query nên phải thao tác trực tiếp với MongoDB
                    UpdateDefinition<TopTrack> updateDefinition = Builders<TopTrack>.Update.Inc(topTrack => topTrack.TrackInfo.FirstMatchingElement().StreamCount, playedTrackCount);
                    await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(filterDefinition, updateDefinition);

                }
                // Xóa key đã cập nhật
                await _redis.KeyDeleteAsync(key);

                return;
            }

        }
    }
}
