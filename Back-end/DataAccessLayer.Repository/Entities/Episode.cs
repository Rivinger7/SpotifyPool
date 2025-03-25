using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities;

public class Episode
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
        
    [BsonElement("podcastId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId PodcastId { get; set; }
        
    [BsonElement("title")]
    public string Title { get; set; }
        
    [BsonElement("description")]
    public string Description { get; set; }
        
    [BsonElement("audioUrl")]
    public string AudioUrl { get; set; }
        
    [BsonElement("duration")]
    public double Duration { get; set; }
        
    [BsonElement("releaseInfo")]
    public ReleaseInfo ReleaseInfo { get; set; }
        
    [BsonElement("images")]
    public List<Image> Images { get; set; } = new List<Image>();
        
    [BsonElement("restriction")]
    public object Restriction { get; set; }
        
    [BsonElement("streamCount")]
    public int StreamCount { get; set; }
        
    [BsonElement("downloadCount")]
    public int DownloadCount { get; set; }
        
    [BsonElement("favoriteCount")]
    public int FavoriteCount { get; set; }
        
    [BsonElement("createdTime")]
    public DateTime CreatedTime { get; set; }
        
    [BsonElement("lastUpdatedTime")]
    public DateTime LastUpdatedTime { get; set; }
        
    [BsonElement("deletedTime")]
    public DateTime? DeletedTime { get; set; }
}