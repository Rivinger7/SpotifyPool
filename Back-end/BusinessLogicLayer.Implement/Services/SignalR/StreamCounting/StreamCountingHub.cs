using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.SignalR.StreamCounting
{
    public class StreamCountingHub(IUnitOfWork unitOfWork, IConnectionMultiplexer redis) : Hub
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDatabase _redis = redis.GetDatabase();

        //public async Task UpdateStreamCountAsync(string trackId)
        //{
        //    // Lấy thông tin user từ Context
        //    string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    // Nếu không có thông tin user thì không thực hiện gì cả
        //    if (userId is null)
        //    {
        //        // Nên thông báo lỗi ở đây
        //        await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to create playlist!");
        //        return;
        //    }

        //    // Cập nhật Stream Count của track
        //    UpdateDefinition<Track> streamCountTrackUpdateDefinition = Builders<Track>.Update.Inc(track => track.StreamCount, 1);
        //    UpdateResult trackUpdateResult = await _unitOfWork.GetCollection<Track>().UpdateOneAsync(track => track.Id == trackId, streamCountTrackUpdateDefinition);
        //}

        public async Task UpdateStreamCountAsync(string trackId)
        {
            string userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
