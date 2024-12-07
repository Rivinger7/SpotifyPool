using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response
{
    public class PlaylistReponseModel
    {
        // Thumbnail
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required IEnumerable<ImageResponseModel> Images { get; set; }

        // User
        public required string UserId { get; set; }
        public required string DisplayName { get; set; }
        public required ImageResponseModel Avatar { get; set; }

        // Total Tracks
        public required int TotalTracks { get; set; }

        // Playlist Item
        public required IEnumerable<TrackResponseModel> Tracks { get; set; }
    }
}
