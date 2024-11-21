using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using SetupLayer.Enum.Services.Playlist;
using Utility.Coding;

namespace DataAccessLayer.Repository.Entities
{
    public class Playlist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public required string Name { get; set; }
        public string? Description { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserID { get; set; }

        public List<PlaylistTracksInfo> TrackIds { get; set; } = [];
        public List<Image> Images { get; set; } = [];

        public DateTime CreatedTime { get; set; }
    }

    public class PlaylistTracksInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string TrackId { get; set; } = null!;
        public DateTime AddedTime { get; set; }
    }
}
