using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request
{
    public class ArtistRequest
    {
        public string Name { get; set; } = default!;
        public IFormFile? ImageFile { get; set; }
    }
}
