using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response
{
    public class ArtistResponseModel
    {
        public required string Name { get; set; }
        public required int Followers { get; set; }
        public required IEnumerable<string> GenreIds { get; set; }
        public required IEnumerable<ImageResponseModel> Images { get; set; }
    }
}
