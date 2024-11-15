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

        public string? Description { get; set; }
        public List<Image>? Images { get; set; } = [];
        public PlaylistName Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserID { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? TrackIds { get; set; } = [];

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedTime { get; set; } = Util.GetUtcPlus7Time();

        public bool IsPinned { get; set; } = false;
	}
}
