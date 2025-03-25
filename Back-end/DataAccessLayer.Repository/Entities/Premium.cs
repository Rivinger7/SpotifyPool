using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccessLayer.Repository.Entities
{
    public class Premium
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public int Duration { get; set; }
    }
}
