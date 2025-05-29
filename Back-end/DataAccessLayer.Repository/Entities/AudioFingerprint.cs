using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class AudioFingerprint
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Biterate = "Unknown";
        public List<byte[]> CompressedFingerprints { get; set; } = [];
        public List<uint> SequenceNumbers { get; set; } = [];
        public List<float> StartsAt { get; set; } = [];
        public List<byte[]> OriginalPoints { get; set; } = [];
        public double Duration { get; set; } // Duration in seconds
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
