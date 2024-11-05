using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

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
        public List<string> ArtistIds { get; set; } = [];
        public int? Popularity { get; set; }
        public required string PreviewURL { get; set; }
        public required int Duration { get; set; }
        public List<Image> Images { get; set; } = [];

        public required bool IsExplicit { get; set; } // Spotify API
        public required bool IsPlayable { get; set; } // Spotify API
        public required string UploadDate { get; set; }
        public required string UploadBy { get; set; }

        public long StreamCount { get; set; } // Real counting
        public long PlayCount { get; set; } // Fake counting
        public long DownloadCount { get; set; }
        public long FavoriteCount { get; set; }

        public required string AudioFeaturesId { get; set; }
    }
}
