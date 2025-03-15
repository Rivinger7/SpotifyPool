using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using DataAccessLayer.Repository.Entities;
using SetupLayer.Enum.Services.Album;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response
{
    public class AlbumResponseModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public IEnumerable<ImageResponseModel> Images { get; set; } = [];
        public ReleaseMetadataResponse ReleaseInfo { get; set; } = null!;
    }
    public class ReleaseMetadataResponse
    {
        public string? ReleasedTime { get; set; }
        public ReleaseStatus Reason { get; set; }
    }
}
