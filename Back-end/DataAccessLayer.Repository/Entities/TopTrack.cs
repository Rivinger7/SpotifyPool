using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class TopTrack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string? Description { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<TopTracksInfo> TrackIds { get; set; } = [];

        public DateTime CreatedTime { get; set; }
    }

    public class TopTracksInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string TrackId { get; set; } = null!;

        public int StreamCount { get; set; }
    }
}
