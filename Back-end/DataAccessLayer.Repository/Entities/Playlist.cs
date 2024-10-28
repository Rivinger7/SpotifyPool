using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using SetupLayer.Enum.Services.Playlist;

namespace DataAccessLayer.Repository.Entities
{
    public class Playlist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string? Description { get; set; }
        public List<Image> Images { get; set; } = [];
        public PlaylistName Name { get; set; }
        public required string UserID { get; set; }
        public List<string> TrackIds { get; set; } = [];
        public DateTime CreatedTime { get; set; }
    }
}
