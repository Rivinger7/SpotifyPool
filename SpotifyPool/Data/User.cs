using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SpotifyPool.Data
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string Birthdate { get; set; }

	}
}
