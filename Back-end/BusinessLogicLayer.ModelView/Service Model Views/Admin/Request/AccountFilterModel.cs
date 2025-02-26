using SetupLayer.Enum.Services.User;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request
{
	public class AccountFilterModel
	{
		public string? UserName { get; set; }

		public string? Email { get; set; }

		public bool? DisplayName { get; set; }

		public UserStatus? Status { get; set; }
	}
}
