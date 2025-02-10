using DataAccessLayer.Repository.Entities;
using SetupLayer.Enum.Services.User;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response
{
	public class AdminDetailResponse
	{
		public string UserId { get; set; } = null!;

		public string UserName { get; set; } = null!;

		public string DisplayName { get; set; } = null!;

		public UserGender Gender { get; set; }

		public string? Birthdate { get; set; }

		public string? PhoneNumber { get; set; }

		public string Email { get; set; } = null!;

		public string? Status { get; set; }

		public List<string> Roles { get; set; } = new List<string>();

		public List<Image> Images { get; set; } = [];

		public DateTime CreatedTime { get; set; }

		public DateTime? LastLoginTime { get; set; }

		public DateTime? LastUpdatedTime { get; set; }
	}
}
