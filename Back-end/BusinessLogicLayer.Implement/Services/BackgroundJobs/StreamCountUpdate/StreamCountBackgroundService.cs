using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using StackExchange.Redis;
using Utility.Coding;

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
                    using IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await UpdateListenedTrack(unitOfWork);
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task UpdateCountToMongoDBAsync(RedisKey key, IUnitOfWork unitOfWork)
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
                    TopTrack topTrack = await unitOfWork.GetCollection<TopTrack>()
                        .Find(topTrack => topTrack.UserId == userId)
                        .FirstOrDefaultAsync();

                   // Nếu không có topTrack thì tạo mới
                   if (topTrack is null)
                   {
                       TopTrack newTopTrack = new()
                       {
                           UserId = userId,
                           TrackInfo =
                           [
                               new()
                           {
                               TrackId = trackId,
                               StreamCount = playedTrackCount
                           }
                           ],
                           CreatedTime = Util.GetUtcPlus7Time(),

                       };

                        // Lưu thông tin topTrack mới
                        await unitOfWork.GetCollection<TopTrack>().InsertOneAsync(newTopTrack);
                        await UpdateInfoToRedis(key, trackId, playedTrackCount);

                        continue;
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
                        await unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(topTrack => topTrack.UserId == userId, addToSetUpdateDefinition);
                        await UpdateInfoToRedis(key, trackId, playedTrackCount);

                        continue;
                    }

                    // Nếu đã tồn tại thì cập nhật Stream Count của topTrackInfo
                    // Filter topTrackInfo cần cập nhật
                    FilterDefinition<TopTrack> filterDefinition = Builders<TopTrack>.Filter.And(
                        Builders<TopTrack>.Filter.Eq(topTrack => topTrack.UserId, userId),
                       Builders<TopTrack>.Filter.ElemMatch(topTrack => topTrack.TrackInfo, track => track.TrackId == trackId));

                    // Cập nhật Stream Count của topTrackInfo
                    // Do MongoDB không hỗ trợ hoàn toàn các Linq query nên phải thao tác trực tiếp với MongoDB
                    UpdateDefinition<TopTrack> updateDefinition = Builders<TopTrack>.Update.Inc(topTrack => topTrack.TrackInfo.FirstMatchingElement().StreamCount, playedTrackCount);
                    var updateResult = await unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(filterDefinition, updateDefinition);

                    // Only reset the Redis value if MongoDB update was successful
                    await UpdateInfoToRedis(key, trackId, playedTrackCount);
                }
            }
            return;
        }


        private async Task UpdateInfoToRedis(RedisKey key, RedisValue field, int count)
        {
            // Cập nhật lại giá trị của field trong key và set lại thời gian TTL cho key
            await _redis.HashDecrementAsync(key, field, count);
            await _redis.KeyExpireAsync(key, TimeSpan.FromMinutes(30));
        }



        private async Task UpdateListenedTrack(IUnitOfWork unitOfWork)
        {
            //lấy tất cả các key có dạng stream_count:* và gọi hàm update từng value của key đó vào mongodb
            var tasks = server.Keys(pattern: "stream_count:*")
                      .Select(key => UpdateCountToMongoDBAsync(key, unitOfWork));

            await Task.WhenAll(tasks);
        }



        ////hàm đếm tổng số lượng nghe của các track của các user từ redis ra
        //private async Task<int> CountAvailableListeningTrack()
        //{
        //    //luaScript
        //    string luaScript = @"
        //            local keys = redis.call('keys', 'streamcount:*')
        //            local totalCount = 0
        //            for _, key in ipairs(keys) do
        //                local hashKeys = redis.call('HKEYS', key)
        //                totalCount = totalCount + #hashKeys
        //            end
        //            return totalCount";

        //    RedisResult result = await _redis.ScriptEvaluateAsync(luaScript);
        //    return (int)result;
        //}
    }
}
