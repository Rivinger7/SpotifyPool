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
    public class StreamCountBackgroundService(IServiceScopeFactory factory) : BackgroundService
    {
        private readonly IServiceScopeFactory _factory = factory;

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


            return;
        }


        private async Task UpdateInfoToRedis(RedisKey key, RedisValue field, int count)
        {
            // Cập nhật lại giá trị của field trong key và set lại thời gian TTL cho key
        }



        private async Task UpdateListenedTrack(IUnitOfWork unitOfWork)
        {
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
