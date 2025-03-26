using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SetupLayer.Enum.Services.User;

namespace DataAccessLayer.Repository.Entities
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public required string PremiumId { get; set; } = null!;
        public Snapshot SnapshotInfo { get; set; } = null!;
        public string Status { get; set; } = null!; // "PAID", "CANCELLED", "PENDING" (pending: chờ - có thể đã hết hạn )
        public long OrderCode { get; set; } 
    }
    public class Snapshot
    {
        // User Info
        public required string DisplayName { get; set; }
        public string Email { get; set; } = null!;
        public string CountryId { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public List<Image> Images { get; set; } = [];
        public string? Birthdate { get; set; }
        public UserGender Gender { get; set; }
        public DateTime? BuyedTime { get; set; } // check hết hạn link thanh toán ở đây (mỗi link thanh toán tồn tại 15p)
        public DateTime? ExpiredTime { get; set; }
        // Premium Info
        public string PremiumName { get; set; } = null!;
        public double PremiumPrice { get; set; }
        public int PremiumDuration { get; set; }
    }
}
