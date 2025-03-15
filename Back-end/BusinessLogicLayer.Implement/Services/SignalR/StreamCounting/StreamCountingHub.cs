using BusinessLogicLayer.Implement.CustomExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.SignalR.StreamCounting
{
    public class StreamCountingHub(IHttpContextAccessor httpContextAccessor, IConnectionMultiplexer connectionMultiplexer) : Hub
    {
        private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task UpdateStreamCountAsync(string trackId)
        {
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? throw new UnAuthorizedCustomException("Your session is limit, you must login again to edit profile!");

            //đặt tên key
            string key = $"stream_count:{userID}";

            //nếu key đã tồn tại 1 field y chang thì chỉ việc tăng giá trị của field đó lên 1; còn chưa có field đó thì vừa tạo field vừa set thời gian TTL cho field đó
            if (await _redis.HashExistsAsync(key, trackId))
            {
                await _redis.HashIncrementAsync(key, trackId, 1);
            }
            else
            {
                await _redis.HashIncrementAsync(key, trackId, 1);
                RedisValue[] fieldValues = [trackId];
                await _redis.HashFieldExpireAsync(key, fieldValues, TimeSpan.FromMinutes(6));
            }

            //set TTL cho key
            await _redis.KeyExpireAsync(key, TimeSpan.FromMinutes(30));

            return;
        }
    }
}
