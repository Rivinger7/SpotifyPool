using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Repository.Entities
{
    public class Image
    {
        [BsonElement("url")] public string? URL { get; set; }
        [BsonElement("height")] public int Height { get; set; }
        [BsonElement("width")] public int Width { get; set; }
    }
}