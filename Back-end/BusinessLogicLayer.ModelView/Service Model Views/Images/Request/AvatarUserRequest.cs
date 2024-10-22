namespace BusinessLogicLayer.ModelView.Service_Model_Views.Images.Request
{
	public class AvatarUserRequest
	{
		public string? URL { get; set; }
		public int Height { get; set; } = 500;
		public int Width { get; set; } = 313;
	}
}
