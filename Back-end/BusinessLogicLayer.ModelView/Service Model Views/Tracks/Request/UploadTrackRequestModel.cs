using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request
{
    public class UploadTrackRequestModel
    {
        public required IFormFile File { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Lyrics { get; set; }
        //public required string PreviewURL { get; set; }
        //public required int Duration { get; set; }
        //public required bool IsExplicit { get; set; } = false;
        public DataAccessLayer.Repository.Entities.Restrictions Restrictions { get; set; }
    }
}
