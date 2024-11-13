using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.User
{
    public enum UserGender
    {
        [EnumMember(Value = "Male")]
        Male,
        [EnumMember(Value = "Female")]
        Female,
        [EnumMember(Value = "Other")]
        Other,
        [EnumMember(Value = "Not Specified")]
        NotSpecified,
    }
}
