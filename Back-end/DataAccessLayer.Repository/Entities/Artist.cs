using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccessLayer.Repository.Entities
{
    public class Artist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string? SpotifyId { get; set; }
        public string Name { get; set; } = null!;
        public int Followers { get; set; }
        public int Popularity { get; set; }
        public List<string> GenreIds { get; set; } = [];
        public List<Image> Images { get; set; } = [];
    }
}
