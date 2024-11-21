using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using System.Text.Json.Serialization;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response
{
    public class TrackResponseModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Lyrics { get; set; }
        public required string PreviewURL { get; set; }
        public required int Duration { get; set; }
        public required string DurationFormated { get; set; }
        public required IEnumerable<ImageResponseModel> Images { get; set; }
        public required IEnumerable<ArtistResponseModel> Artists { get; set; }

        // FaveoriteSongs
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public required string AddedTime { get; set; }
    }
}
