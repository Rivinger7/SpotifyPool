using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SetupLayer.Enum.Services.User;

namespace DataAccessLayer.Repository.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;

        public UserRole Role {  get; set; }
        public UserProduct Product { get; set; }
        public string CountryId { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public required string DisplayName { get; set; }
        public int Followers { get; set; }
        public List<Image> Images { get; set; } = [];
        public DateOnly? Birthdate { get; set; }
        public string? Gender { get; set; }
        public UserStatus Status { get; set; }
        public string? TokenEmailConfirm { get; set; }
        public bool? IsLinkedWithGoogle { get; set; } = null;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
