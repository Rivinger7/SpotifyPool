using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DataAccessLayer.Repository.Entities
{
    public class Podcast
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        
        [BsonElement("title")]
        public string Title { get; set; }
        
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("publisher")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Publisher { get; set; }
        
        [BsonElement("host")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Host { get; set; }
        
        [BsonElement("categories")]
        public List<string> Categories { get; set; } = new List<string>();
        
        [BsonElement("images")]
        public List<Image> Images { get; set; } = new List<Image>();
        
        [BsonElement("popularity")]
        public int Popularity { get; set; }
        
        [BsonElement("followersCount")]
        public int FollowersCount { get; set; }
        
        [BsonElement("createdTime")]
        public DateTime CreatedTime { get; set; }
        
        [BsonElement("lastUpdatedTime")]
        public DateTime LastUpdatedTime { get; set; }
        
        [BsonElement("deletedTime")]
        public DateTime? DeletedTime { get; set; }
        
        [BsonElement("episodes")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> Episodes { get; set; } = new List<ObjectId>();
    }
}