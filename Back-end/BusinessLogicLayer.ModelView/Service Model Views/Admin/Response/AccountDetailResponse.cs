using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response
{
	public class AccountDetailResponse
	{
		public string UserId { get; set; } = null!;

		public string UserName { get; set; } = null!;

		public string DisplayName { get; set; } = null!;

		public string Gender { get; set; } = null!;

		public string? Birthdate { get; set; }

		public string? PhoneNumber { get; set; }

		public string CountryId { get; set; } = null!;

		public int Followers { get; set; }

		public string Email { get; set; } = null!;

		public string? Status { get; set; }

		public string Product { get; set; } = null!;

		public List<string> Roles { get; set; } = new List<string>();

		public List<Image> Images { get; set; } = [];

		public string CreatedTime { get; set; } = null!;

		public string? LastLoginTime { get; set; }

		public string? LastUpdatedTime { get; set; }
	}
}
