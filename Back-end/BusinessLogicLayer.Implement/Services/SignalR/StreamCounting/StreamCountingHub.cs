using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.SignalR.StreamCounting
{
    public class StreamCountingHub(IHttpContextAccessor httpContext, IConnectionMultiplexer redis) : Hub
    {
        private readonly IHttpContextAccessor _httpContext = httpContext;
        private readonly IDatabase _redis = redis.GetDatabase();

        public async Task UpdateStreamCountAsync(string trackId)
        {
            string userId = _httpContext.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to create playlist!");
                return;
            }

            string key = $"stream_count:{userId}";
            await _redis.HashIncrementAsync(key, trackId, 1);
            return;
        }

    }
}
