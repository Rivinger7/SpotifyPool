using System.Text.Json.Serialization;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response
{
    public class AuthenticatedResponseModel
    {
        // This field will only be included in the JSON response if it is not null
        // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AccessToken { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ConfirmationLinkWithGoogleAccount { get; set; }
    }
}
