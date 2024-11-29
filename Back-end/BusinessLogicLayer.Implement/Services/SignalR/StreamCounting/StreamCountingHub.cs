using BusinessLogicLayer.Implement.CustomExceptions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace BusinessLogicLayer.Implement.Services.SignalR.StreamCounting
{
    public class StreamCountingHub(IUnitOfWork unitOfWork) : Hub
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task UpdateStreamCountAsync(string trackId)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.Identity?.Name;

            if (IsValidateData(userId, trackId))
            {
                return;
            }

            UpdateDefinition<Track> updateDefinition = Builders<Track>.Update.Inc(track => track.StreamCount, 1);
            UpdateResult trackUpdateResult = await _unitOfWork.GetCollection<Track>().UpdateOneAsync(track => track.Id == trackId, updateDefinition);

            // Ngắt kết nối
            Context.Abort();

            //FilterDefinition<TopTrack> filterDefinitionBuilder = Builders<TopTrack>.Filter.Eq(topTrack => topTrack.UserId, userId);
            //UpdateDefinition<TopTrack> updateDefinitionTopTrack = Builders<TopTrack>.Update.AddToSet(topTrack => topTrack.TrackIds, trackId);

            //await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(filterDefinitionBuilder, updateDefinitionTopTrack, new UpdateOptions { IsUpsert = true });
        }

        private static bool IsValidateData(string? userId = "xxx", string? trackId = "xxx")
        {
            return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(trackId);
        }
    }
}
