﻿using System.Runtime.Serialization;

namespace SetupLayer.Enum.Microservices.Cloudinary
{
    public enum ImageTag
    {
        [EnumMember(Value = "Users_Profile")]
        Users_Profile,
        [EnumMember(Value = "Artist")]
        Artist,
        [EnumMember(Value = "Test")]
        Test,
        [EnumMember(Value = "Playlist")]
		Playlist,
        [EnumMember(Value = "Spectrogram")]
        Spectrogram

    }
}
