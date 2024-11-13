using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.User
{
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
