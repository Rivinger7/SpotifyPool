using DataAccessLayer.Repository.Entities;
using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response
{
    public class TrackResponseModel
    {
        public string? TrackId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Popularity { get; set; }
        public string? PreviewURL { get; set; }
        public int? Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<Image> Images { get; set; } = [];
        public List<ArtistDetails> Artists { get; set; } = [];
        public List<AvailableMarkets> AvailableMarkets { get; set; } = [];
    }

    public class SpotifyTrack
    {
        [JsonProperty("items")]
        public required List<Item> Items { get; set; }
    }

    public class Item
    {
        [JsonProperty("track")]
        public required TrackDetails TrackDetails { get; set; }
    }

    public class TrackDetails
    {
        [JsonProperty("id")]
        public string? TrackId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("duration_ms")]
        public int? Duration { get; set; }

        [JsonProperty("popularity")]
        public int? Popularity { get; set; }

        [JsonProperty("preview_url")] // MP3
        public string? PreviewUrl { get; set; }

        [JsonProperty("release_date")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("album")]
        public required AlbumDetails AlbumDetails { get; set; }

        [JsonProperty("artists")]
        public required List<ArtistDetails> Artists { get; set; }

        [JsonProperty("available_markets")]
        public required List<string> AvailableMarkets { get; set; }
    }

    public class AlbumDetails
    {
        //[JsonProperty("id")]
        //public string? SpotifyId { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; } = [];
    }

    public class ArtistDetails
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class AvailableMarkets
    {
        public string Id { get; set; } = null!;
    }
}
