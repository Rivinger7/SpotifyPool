using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccessLayer.Repository.Entities
{
    public class Test
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime DateTimeValue { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime DateOnly { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string DateTimeString { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string DateTimeString2 { get; set; }
    }
}
