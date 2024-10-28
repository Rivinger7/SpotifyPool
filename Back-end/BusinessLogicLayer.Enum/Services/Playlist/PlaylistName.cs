using System.Runtime.Serialization;

namespace SetupLayer.Enum.Services.Playlist
{
    public enum PlaylistName
    {
        [EnumMember(Value = "Favorite Songs")]
        FavoriteSong,
        [EnumMember(Value = "Weekly Songs")]
        WeeklySong
    }
}
