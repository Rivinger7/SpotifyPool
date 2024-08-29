using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Data_Access_Layer.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? Phonenumber { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; } = null!;
        public DateOnly? Birthdate { get; set; }
        public string? Image { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; } = null!;
        public string? Token { get; set; }
        public bool? isLinkedWithGoogle { get; set; } = null;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
