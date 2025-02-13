using DataAccessLayer.Repository.Entities;
using SetupLayer.Enum.Services.User;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request
{
	public class UpdateUserRequest
	{
		[Required(ErrorMessage = "Display Name cannot be left blank")]
		[StringLength(30, ErrorMessage = "Display name cannot exceed 30 characters")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "The display name must contain only letters and numbers, and must begin with a letter..")]
		public string DisplayName { get; set; } = null!;

		[Required(ErrorMessage = "Gender cannot be left blank")]
		public UserGender Gender { get; set; }

		public string? Birthdate { get; set; }

		public string? PhoneNumber { get; set; }

		[Required(ErrorMessage = "Email cannot be left blank")]
		public string Email { get; set; } = null!;

		public int Followers { get; set; }

		[Required(ErrorMessage = "Product cannot be left blank")]
		public UserProduct Product { get; set; }

		[Required(ErrorMessage = "Role cannot be left blank")]
		public List<UserRole> Roles { get; set; } = new List<UserRole>();

		public List<Image> Images { get; set; } = [];
	}
}
