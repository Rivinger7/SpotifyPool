using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Genres.Response
{
    public class GenreResponseModel
    {
        [JsonProperty("genres")]
        public List<string> Name { get; set; } = null!;
    }
}
