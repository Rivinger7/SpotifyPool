using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response
{
    public class UpFetchResponseModel
    {
        [JsonProperty("tracks")]
        public required List<ItemUpFetch> ItemUF { get; set; }
    }

    public class ItemUpFetch
    {
        [JsonProperty("artists")]
        public required List<ArtistDetail> ArtistDetail { get; set; }
        [JsonProperty("id")]
        public required string TrackSpotifyId { get; set; }
    }

    public class ArtistDetail
    {
        [JsonProperty("id")]
        public required string ArtistSpotifyId { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}
