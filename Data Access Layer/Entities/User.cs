using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Data_Access_Layer.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Birthdate { get; set; }
        public string? Image { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; }
        public string? Token { get; set; }
        public bool isLinkedWithGoogle { get; set; }
    }
}
