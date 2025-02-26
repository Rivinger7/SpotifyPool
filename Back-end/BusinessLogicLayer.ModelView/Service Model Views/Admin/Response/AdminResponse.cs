using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response
{
	public class AdminResponse
	{
		public string? UserId { get; set; }

		public string UserName { get; set; } = null!;

		public string? Email { get; set; }

		public List<string> Roles { get; set; } = new List<string>();

		public string? Status { get; set; }

		public List<Image> Images { get; set; } = [];

		//public UserStatus? Status { get; set; }
		//public string StatusDescription => Status.HasValue ? EnumHelper.Name(Status.Value) : "Không có trạng thái";
	}
}
