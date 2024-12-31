using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request
{
    public class PlaylistRequestModel
    {
        public string PlaylistName { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}
