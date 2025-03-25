using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.ContentManagers;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Services.Album;
using SetupLayer.Enum.Services.Track;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.ContentManagers
{
    public class ContentManagersBLL : IContentManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CloudinaryService _cloudinaryService;

        public ContentManagersBLL(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryService = cloudinaryService;
        }

        public async Task ChangeTrackRestrictionAsync(string trackId, TrackRestrictionRequestModel model)
        {
            FilterDefinition<Track> filter = Builders<Track>.Filter.Eq(a => a.Id, trackId);
            // Lấy track hiện tại
            Track existingTrack = await _unitOfWork.GetCollection<Track>()
                .Find(a => a.Id == trackId)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Track with ID {trackId} does not exist.");
            Restrictions newRestriction;
            if(model.Reason == RestrictionReason.None)
            {
                newRestriction = new Restrictions()
                {
                    IsPlayable = true,
                    Reason = model.Reason,
                    Description = model.ReasonDescription,
                    RestrictionDate = null
                };
            } 
            else
            {
                newRestriction = new Restrictions()
                {
                    IsPlayable = false,
                    Reason = model.Reason,
                    Description = model.ReasonDescription,
                    RestrictionDate = Util.GetUtcPlus7Time()
                };
                if (model.Reason == RestrictionReason.Pending)
                {
                    newRestriction.RestrictionDate = null;
                }
            }
            existingTrack.Restrictions = newRestriction;
            //Chuyển thành BsonDocument để cập nhật, loại bỏ _id
            BsonDocument bsonDoc = existingTrack.ToBsonDocument();
            bsonDoc.Remove("_id");
            //Tạo UpdateDefinition từ BsonDocument
            BsonDocument update = new BsonDocument("$set", bsonDoc);
            UpdateResult result = await _unitOfWork.GetCollection<Track>()
                .UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException($"Track with ID {trackId} does not exist.");
            }
        }
    }
}
