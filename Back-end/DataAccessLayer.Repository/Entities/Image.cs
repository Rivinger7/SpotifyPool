using Newtonsoft.Json;

namespace DataAccessLayer.Repository.Entities
{
    public class Image
    {
        [JsonProperty("url")]
        public string URL { get; set; } = null!;

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
