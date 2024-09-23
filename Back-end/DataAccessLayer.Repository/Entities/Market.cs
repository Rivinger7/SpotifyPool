using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccessLayer.Repository.Entities
{
    public class Market
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
    }
}
