using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BusinessLogicLayer.Implement.Services.SignalR.StreamCounting
{
    public class StreamCountingHub(IUnitOfWork unitOfWork) : Hub
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task UpdateStreamCountAsync(string trackId)
        {
            // Lấy thông tin user từ Context
            string? userId = "6736c563216626b7bf5f1441";

			string? topTrackId = await _unitOfWork.GetCollection<TopTrack>()
                                                 .Find(topTrack => topTrack.UserId == userId) //&& topTrack.TrackInfo.Any(track => track.TrackId == trackId))
	                                             .Project(topTrack => topTrack.TopTrackId)
	                                             .FirstOrDefaultAsync();

			if (topTrackId is null)
            {

				// create new
				TopTrack newtopTrack = new ()
				{
					UserId = userId,
					TrackInfo =
					[
						new TopTracksInfo
						{
							TrackId = trackId,
							StreamCount = 1
						}
					],
				};
				await _unitOfWork.GetCollection<TopTrack>().InsertOneAsync(newtopTrack);
				return;
			}


            TopTracksInfo? trackInfo = await _unitOfWork.GetCollection<TopTrack>()
                                             .Find(topTrack => topTrack.TopTrackId == topTrackId && topTrack.TrackInfo.Any(track => track.TrackId == trackId))
                                             .Project(topTrack => topTrack.TrackInfo.FirstOrDefault(track => track.TrackId == trackId))
                                             .FirstOrDefaultAsync();

            if (trackInfo is null){
                // add new track
                FilterDefinition<TopTrack> addTrackInfofilter = Builders<TopTrack>.Filter.Eq(topTrack => topTrack.TopTrackId, topTrackId);

                UpdateDefinition<TopTrack> addTrackInfoUpdateDefinition = Builders<TopTrack>.Update.Push(topTrack => topTrack.TrackInfo, new TopTracksInfo
                {
                    TrackId = trackId,
                    StreamCount = 1
                });
                UpdateOptions addTrackInfoUpdateOptions = new() { IsUpsert = false };

                await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(addTrackInfofilter, addTrackInfoUpdateDefinition, addTrackInfoUpdateOptions);
                return;
            }

            var filter = Builders<TopTrack>.Filter.And(
                Builders<TopTrack>.Filter.Eq(topTrack => topTrack.TopTrackId, topTrackId),
                Builders<TopTrack>.Filter.ElemMatch(topTrack => topTrack.TrackInfo, track => track.TrackId == trackId)
            );

            var updateDefinition = Builders<TopTrack>.Update.Inc("TrackInfo.$.StreamCount", 1);
            var updateOptions = new UpdateOptions { IsUpsert = false };

            UpdateResult trackUpdateResult = await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(filter, updateDefinition, updateOptions);
        }

        private static bool IsValidateData(string? userId = "xxx", string? trackId = "xxx")
        {
            return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(trackId);
        }
    }
}
