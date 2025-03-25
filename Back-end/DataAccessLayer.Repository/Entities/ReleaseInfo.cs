using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities;

public class ReleaseInfo
{
    [BsonElement("releasedTime")]
    public DateTime ReleasedTime { get; set; }
        
    [BsonElement("datetime")]
    public DateTime Datetime { get; set; }
        
    [BsonElement("reason")]
    public string Reason { get; set; }
}