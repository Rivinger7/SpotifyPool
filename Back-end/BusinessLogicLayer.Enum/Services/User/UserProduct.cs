using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BusinessLogicLayer.Enum.Services.User
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserProduct
    {
        [EnumMember(Value = "Free")]
        Free,
        [EnumMember(Value = "Premium")]
        Premium,
        [EnumMember(Value = "Royal")]
        Royal
    }
}
