using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class TopTrack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }

        public List<TopTrackInfo> TrackInfo { get; set; } = [];

        public required DateTime CreatedTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }

        public TopTrack()
        {
            
        }
    }

    public class TopTrackInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string TrackId { get; set; } = null!;

        public int StreamCount { get; set; }
    }
}
