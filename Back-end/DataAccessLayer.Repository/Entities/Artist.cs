using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccessLayer.Repository.Entities
{
    public class Artist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        public string? SpotifyId { get; set; }
        public string Name { get; set; } = null!;
        public int Followers { get; set; }
        public int Popularity { get; set; }
        public List<Image> Images { get; set; } = [];
        public required DateTime CreatedTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
