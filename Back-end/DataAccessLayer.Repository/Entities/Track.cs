using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccessLayer.Repository.Entities
{
    public class Track
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string SpotifyId { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<string> ArtistIds { get; set; } = [];
        public int Popularity { get; set; }
        public string? PreviewURL { get; set; }
        public int Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<Image> Images { get; set; } = [];
        public List<string> AlbumIds { get; set; } = [];
        public List<string> MarketsIds { get; set; } = [];
    }
}
