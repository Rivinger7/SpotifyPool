using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request
{
	public class UpdatePlaylistRequestModel
	{
		public string Name { get; set; }
		public string? Description { get; set; }
		public IFormFile? Image { get; set; }

	}
}
