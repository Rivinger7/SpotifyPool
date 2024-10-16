using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class OTP{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Email { get; set; }
        public string OTPCode { get; set; }
        public DateTimeOffset ExpiryTime { get; set; }
        public bool IsUsed { get; set; }
    }
}