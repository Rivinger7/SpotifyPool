using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.Track
{
    public enum RestrictionReason
    {
        [EnumMember(Value = "market")]
        Market,
        [EnumMember(Value = "product")]
        Product,
        [EnumMember(Value = "explicit")]
        Explicit,
        [EnumMember(Value = "Other")]
        Other,
        [EnumMember(Value = "None")]
        None
    }
}
