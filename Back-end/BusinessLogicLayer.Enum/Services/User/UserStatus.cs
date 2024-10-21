using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SetupLayer.Enum.Services.User
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserStatus
    {
        [EnumMember(Value = "Inactive")]
        Inactive,
        [EnumMember(Value = "Active")]
        Active,
        [EnumMember(Value = "Banned")]
        Banned
    }
}
