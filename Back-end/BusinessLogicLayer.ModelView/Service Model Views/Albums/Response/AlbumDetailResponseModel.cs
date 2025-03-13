using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response
{
    public class AlbumDetailResponseModel
    {
        // Info
        public AlbumResponseModel Info { get; set; } = null!;

        // Artist Info (Who create this album)
        public ArtistResponseModel CreatedBy { get; set; } = null!;

        // List Artists tag in this album
        public List<ArtistResponseModel> Artists { get; set; } = [];

        // Album items
        public IEnumerable<string> TrackIds { get; set; } = [];
        public IEnumerable<TrackResponseModel> Tracks { get; set; } = [];
    }
   
}
