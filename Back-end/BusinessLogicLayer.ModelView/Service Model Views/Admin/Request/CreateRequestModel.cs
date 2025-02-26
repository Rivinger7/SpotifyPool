using DataAccessLayer.Repository.Entities;
using SetupLayer.Enum.Services.User;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request
{
	public class CreateRequestModel
	{
		[Required(ErrorMessage = "Please enter your username")]
		[StringLength(30, ErrorMessage = "Username cannot exceed 30 characters")]
		[MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Username must contain only letters and numbers, and must start with a letter.")]
		public string UserName { get; set; } = null!;

		[Required(ErrorMessage = "Please enter your password")]
		[DataType(DataType.Password)]
		[StringLength(30, ErrorMessage = "Password cannot exceed 30 characters")]
		[MinLength(3, ErrorMessage = "Password must be at least 3 characters")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Password must contain only letters and numbers, and must start with a letter.")]
		public string Password { get; set; } = null!;

		[Required(ErrorMessage = "Please confirm password!!")]
		[DataType(DataType.Password)]
		[StringLength(30, ErrorMessage = "Password cannot exceed 30 characters")]
		[MinLength(3, ErrorMessage = "Password must be at least 3 characters")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Password must contain only letters and numbers, and must start with a letter.")]
		public string ConfirmedPassword { get; set; } = null!;

		[Required(ErrorMessage = "Please enter your display name")]
		[StringLength(30, ErrorMessage = "Display name cannot exceed 30 characters")]
		[MinLength(3, ErrorMessage = "Display name must be least 3 characters")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Display name must contain only letters and numbers, and must start with a letter.")]
		public string DisplayName { get; set; } = null!;

		[Required(ErrorMessage = "Please enter your email")]
		[EmailAddress(ErrorMessage = "Invalid email address")]
		[MinLength(3, ErrorMessage = "Email must be at least 3 characters")]
		[StringLength(50, ErrorMessage = "Email must not exceed 50 characters")]
		public string Email { get; set; } = null!;

		[Phone(ErrorMessage = "Invalid phone number")]
		[RegularExpression(@"^\d{9,11}$", ErrorMessage = "Phone number must be 9 to 11 digits long")]
		public string? PhoneNumber { get; set; }

		[Required(ErrorMessage = "Role cannot be left blank")]
		public List<UserRole> Roles { get; set; } = new List<UserRole>();

		public List<Image> Images { get; set; } = new List<Image>();
	}
}
