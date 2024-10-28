using System.Runtime.Serialization;

namespace SetupLayer.Enum.Microservices.Cloudinary
{
    public enum AudioTagParent
    {
        [EnumMember(Value = "Tracks")]
        Tracks,
        [EnumMember(Value = "Test")]
        Test
    }
}
