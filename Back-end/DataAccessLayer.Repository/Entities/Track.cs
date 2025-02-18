using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class Track
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        public string? SpotifyId { get; set; }

        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Lyrics { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> ArtistIds { get; set; } = [];

        public int? Popularity { get; set; } = 0;
        public required string StreamingUrl { get; set; }
        public required int Duration { get; set; }
        public List<Image> Images { get; set; } = [];

        public required Restrictions Restrictions { get; set; }
        public required string UploadDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public required string UploadBy { get; set; }

        public DateTime? LastUpdatedTime { get; set; }

        public long StreamCount { get; set; } // Real counting
        public long DownloadCount { get; set; }
        public long FavoriteCount { get; set; }
        public required AudioFeatures AudioFeatures { get; set; }
    }
}
