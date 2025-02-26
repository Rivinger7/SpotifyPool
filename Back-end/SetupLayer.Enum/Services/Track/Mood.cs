using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.Track
{
    public enum Mood
    {
        [EnumMember(Value = "sad")]
        Sad,
        [EnumMember(Value = "neutral")]
        Neutral,
        [EnumMember(Value = "happy")]
        Happy,
        [EnumMember(Value = "blisfull")]
        Blisfull,
        [EnumMember(Value = "focus")]
        Focus,
        [EnumMember(Value = "Random")]
        Random
    }
}
