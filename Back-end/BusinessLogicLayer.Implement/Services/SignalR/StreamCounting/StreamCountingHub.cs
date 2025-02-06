using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Security.Claims;

namespace BusinessLogicLayer.Implement.Services.SignalR.StreamCounting
{
    public class StreamCountingHub(IUnitOfWork unitOfWork) : Hub
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task UpdateStreamCountAsync(string trackId)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to create playlist!");
                return;
            }

            // Cập nhật Stream Count của track
            UpdateDefinition<Track> streamCountTrackUpdateDefinition = Builders<Track>.Update.Inc(track => track.StreamCount, 1);
            UpdateResult trackUpdateResult = await _unitOfWork.GetCollection<Track>().UpdateOneAsync(track => track.Id == trackId, streamCountTrackUpdateDefinition);
        }
    }
}
