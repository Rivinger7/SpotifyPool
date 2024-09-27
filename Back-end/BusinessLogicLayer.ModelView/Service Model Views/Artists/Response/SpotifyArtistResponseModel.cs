using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response
{
    public class SpotifyArtistResponseModel
    {
        public string SpotifyId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Followers { get; set; }
        public int Popularity { get; set; }
        public List<string> Genres { get; set; } = [];
        public List<ImageResponseModel> Images { get; set; } = [];
    }

    public class SpotifyArtist
    {
        [JsonProperty("artists")]
        public required List<ArtistDetails> Artists { get; set; }
    }

    public class ArtistDetails
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("followers")]
        public required Followers Followers { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; } = [];

        [JsonProperty("images")]
        public List<ImageResponseModel> Images { get; set; } = [];
    }

    //public class Genres
    //{
    //    public string SpotifyId { get; set; } = null!;
    //}

    public class Followers
    {
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
