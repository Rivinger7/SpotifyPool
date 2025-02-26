using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities.SignalR
{
    public class UserConnection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public List<string> ConnectionIds { get; set; } = [];
        public required string UserId { get; set; }
        public bool IsConnected { get; set; }
        public List<string> Devices { get; set; } = [];

        public string TrackId { get; set; } = default!;
        public double CurrentTime { get; set; }
        public bool IsPlaying { get; set; }
        public DateTime LastUpdated { get; set; }

        public required DateTime CreatedTime { get; set; }
        public required DateTime LastConnected { get; set; }
        public DateTime? LastDisconnected { get; set; }
    }
}
