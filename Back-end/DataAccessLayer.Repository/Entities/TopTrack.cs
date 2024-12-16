using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Utility.Coding;

namespace DataAccessLayer.Repository.Entities
{
    public class TopTrack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TopTrackId { get; set; } = null!;


        //public TopItemType Type { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }


        public List<TopTracksInfo> TrackInfo { get; set; } = [];

		//public int Popularity { get; set; } =0;

    }

    public class TopTracksInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string TrackId { get; set; } = null!;

        public int StreamCount { get; set; }

        public DateTime FirstAccessTime { get; set; } = Util.GetUtcPlus7Time();

        public Track? Track { get; set; }

        public List<Artist>? Artist { get; set; }
    }
}
