using MongoDB.Bson;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role {  get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }

        public Market CountryId { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string Email { get; set; } = null!;
        public int Followers { get; set; }
        public List<Image> Images { get; set; } = [];
        public string Product { get; set; } = null!;
        public DateOnly? Birthdate { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; } = null!;
        public string? TokenEmailConfirm { get; set; }
        public bool? IsLinkedWithGoogle { get; set; } = null;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
