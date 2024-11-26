
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response
{
	public class PlaylistsResponseModel
	{
		public required string Id { get; set; }
		public required string Name { get; set; }
		public required IEnumerable<ImageResponseModel> Images { get; set; }
    }
}
