using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SetupLayer.Enum.Services.User
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        [EnumMember(Value = "Admin")]
        Admin,
        [EnumMember(Value = "Artist")]
        Artist,
        [EnumMember(Value = "Customer")]
        Customer
    }
}
