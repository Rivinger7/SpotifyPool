using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response
{
    public class ArtistResponseModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required int Followers { get; set; }
        public required IEnumerable<string> GenreIds { get; set; }
        public required IEnumerable<ImageResponseModel> Images { get; set; }
    }
}
