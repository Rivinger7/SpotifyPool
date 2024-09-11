using MongoDB.Bson;
using AspNetCore.Identity.MongoDbCore.Models;

namespace Data_Access_Layer.Entities
{
    public class User : MongoIdentityUser<ObjectId>
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public Guid Id { get; set; } = new Guid();
        public string? FullName { get; set; }
        //public string Email { get; set; } = null!;
        //public string? PhoneNumber { get; set; }
        //public string? UserName { get; set; }
        public string Role { get; set; } = null!;
        public DateOnly? Birthdate { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; } = null!;
        public string? Token { get; set; }
        public bool? IsLinkedWithGoogle { get; set; } = null;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Image Object
        public string? Image { get; set; }
        public string? Type { get; set; }
    }
}
