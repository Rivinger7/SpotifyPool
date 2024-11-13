using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.User
{
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
