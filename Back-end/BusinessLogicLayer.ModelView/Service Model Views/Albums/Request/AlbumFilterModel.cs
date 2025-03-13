using SetupLayer.Enum.Services.Album;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request
{
    public class AlbumFilterModel
    {

        public string? CreatedBy { get; set; } // id artist tạo ra album
        public string? Name { get; set; }
        public List<string> ArtistIds { get; set; } = [];
        public DateTime? ReleasedTime { get; set; }
        public ReleaseStatus? Reason { get; set; }
        public bool? IsReleased { get; set; } // ReleasedTime <= now && Reason=Official
        public bool? IsSortByName { get; set; }

    }
}
