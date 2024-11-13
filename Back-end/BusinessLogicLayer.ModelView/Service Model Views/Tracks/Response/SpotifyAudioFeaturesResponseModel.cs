using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response
{
    public class SpotifyAudioFeaturesResponseModel
    {
        public required string Id { get; set; }
        public float Acousticness { get; set; }

        public float Danceability { get; set; }

        public int Duration { get; set; }

        public float Energy { get; set; }

        public float Instrumentalness { get; set; }

        public int Key { get; set; }

        public float Liveness { get; set; }

        public float Loudness { get; set; }

        public int Mode { get; set; }

        public float Speechiness { get; set; }

        public float Tempo { get; set; }

        public int TimeSignature { get; set; }

        public float Valence { get; set; }
    }

    public class SpotifyAudioFeatures
    {
        [JsonProperty("audio_features")]
        public required List<AudioFeaturesResponse> AudioFeatures { get; set; }
    }

    public class AudioFeaturesResponse
    {
        [JsonProperty("acousticness")]
        public float Acousticness { get; set; }

        [JsonProperty("danceability")]
        public float Danceability { get; set; }

        [JsonProperty("duration_ms")]
        public int Duration { get; set; }

        [JsonProperty("energy")]
        public float Energy { get; set; }

        [JsonProperty("instrumentalness")]
        public float Instrumentalness { get; set; }

        [JsonProperty("key")]
        public int Key { get; set; }

        [JsonProperty("liveness")]
        public float Liveness { get; set; }

        [JsonProperty("loudness")]
        public float Loudness { get; set; }

        [JsonProperty("mode")]
        public int Mode { get; set; }

        [JsonProperty("speechiness")]
        public float Speechiness { get; set; }

        [JsonProperty("tempo")]
        public float Tempo { get; set; }

        [JsonProperty("time_signature")]
        public int TimeSignature { get; set; }

        [JsonProperty("valence")]
        public float Valence { get; set; }
    }
}
