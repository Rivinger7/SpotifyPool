using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Entities
{
    public class Album
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string SpotifyId { get; set; } = null!;

        public string Type { get; set; } = null!;
        public int TotalTracks { get; set; }
        public List<string> MarketIds { get; set; } = [];
        public List<Image> Images { get; set; } = [];
        public string Name { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        public List<string> ArtistIds { get; set; } = [];
        public List<string> TrackIds { get; set; } = [];
    }

}
