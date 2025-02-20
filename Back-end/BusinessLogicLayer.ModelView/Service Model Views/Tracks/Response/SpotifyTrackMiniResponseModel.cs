using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response
{
    public class SpotifyTrackMiniResponseModel
    {
        [JsonProperty("album")]
        public SpotifyAlbumResponseModel Album { get; set; }
        [JsonProperty("id")]
        public required string TrackSpotifyId { get; set; }
        [JsonProperty("artists")]
        public List<ArtistMiniDetail> ArtistDetail { get; set; } = [];
    }

    public class SpotifyAlbumResponseModel
    {
        
        [JsonProperty("images")]
        public List<ImageResponseModel> Images { get; set; } = [];
    }

    public class ArtistMiniDetail
    {
        [JsonProperty("id")]
        public required string ArtistSpotifyId { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
        [JsonProperty("followers")]
        public ArtistFollowersMini Followers { get; set; }

        [JsonProperty("images")]
        public List<ImageResponseModel> Images { get; set; } = [];
    }

    public class ArtistFollowersMini
    {
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
