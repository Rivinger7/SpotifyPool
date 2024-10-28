using System.Runtime.Serialization;

namespace SetupLayer.Enum.Microservices.Cloudinary
{
    public enum ImageTag
    {
        [EnumMember(Value = "Users_Profile")]
        Users_Profile,
        [EnumMember(Value = "Test")]
        Test
    }
}
