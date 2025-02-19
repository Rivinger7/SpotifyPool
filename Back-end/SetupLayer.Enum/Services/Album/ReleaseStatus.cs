using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.Album
{
    public enum ReleaseStatus
    {
        [EnumMember(Value = "Not Announced")]
        NotAnnounced,
        [EnumMember(Value = "Delayed")]
        Delayed,
        [EnumMember(Value = "Canceled")]
        Canceled,
        [EnumMember(Value = "Leaked")]
        Leaked,
        [EnumMember(Value = "Official")]
        Official
    }
}
