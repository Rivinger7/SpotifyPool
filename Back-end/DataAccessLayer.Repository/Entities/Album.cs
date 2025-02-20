using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SetupLayer.Enum.Services.Album;

namespace DataAccessLayer.Repository.Entities
{
    public class Album
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public required string CreatedBy { get; set; } = null!;
        public List<string> ArtistIds { get; set; } = [];
        public List<Image> Images { get; set; } = [];
        public List<string> TrackIds { get; set; } = [];

        public required ReleaseMetadata ReleaseInfo { get; set; }
    }

    public class ReleaseMetadata
    {
        public DateTime? ReleasedTime { get; set; }
        public ReleaseStatus Reason { get; set; }
    }
}
