using Microsoft.AspNetCore.Http;
using SetupLayer.Enum.Services.Album;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request
{
    public class AlbumRequestModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public List<string> ArtistIds { get; set; } = [];
        public DateTime? ReleasedTime { get; set; }
        public ReleaseStatus Reason { get; set; }
    }
}
