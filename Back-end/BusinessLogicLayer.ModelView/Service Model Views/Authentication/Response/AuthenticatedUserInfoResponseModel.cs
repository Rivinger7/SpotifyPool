using System.Text.Json.Serialization;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response
{
    public class AuthenticatedUserInfoResponseModel
    {
        public string? AccessToken { get; set; }
        public string? Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ArtistId { get; set; }

        public List<string>? Role { get; set; } = [];
        public string? Name { get; set; }
        public List<string>? Avatar { get; set; } = [];
    }
}
