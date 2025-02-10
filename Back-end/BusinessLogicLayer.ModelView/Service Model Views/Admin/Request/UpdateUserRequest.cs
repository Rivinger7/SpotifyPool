using DataAccessLayer.Repository.Entities;
using SetupLayer.Enum.Services.User;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request
{
	public class UpdateUserRequest
	{
		[Required(ErrorMessage = "Display Name cannot be left blank")]
		[StringLength(30, ErrorMessage = "Display name cannot exceed 30 characters")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên hiển thị chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
		public string DisplayName { get; set; } = null!;

		[Required(ErrorMessage = "Gender cannot be left blank")]
		public UserGender Gender { get; set; }

		public string? Birthdate { get; set; }

		public string? PhoneNumber { get; set; }

		[Required(ErrorMessage = "Email cannot be left blank")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Status cannot be left blank")]
		public UserStatus Status { get; set; }

		public int Followers { get; set; }

		[Required(ErrorMessage = "Product cannot be left blank")]
		public UserProduct Product { get; set; }

		[Required(ErrorMessage = "Role cannot be left blank")]
		public List<UserRole> Roles { get; set; } = new List<UserRole>();

		public List<Image> Images { get; set; } = [];
	}
}
