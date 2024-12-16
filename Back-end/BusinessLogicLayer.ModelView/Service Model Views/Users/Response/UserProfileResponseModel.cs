using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response
{
	public class UserProfileResponseModel
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
		public List<Image> Avatar { get; set; } = [];
    }
}
