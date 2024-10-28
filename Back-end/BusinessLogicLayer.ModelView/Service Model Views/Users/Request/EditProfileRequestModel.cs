using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request
{
	public class EditProfileRequestModel
	{
		[Display(Name = "DisplayName")]
		[StringLength(30, ErrorMessage = "Tên hiển thị không được vượt quá 30 ký tự")]
		[MinLength(3, ErrorMessage = "Tên hiển thị phải có ít nhất 3 ký tự")]
		[RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Tên hiển thị chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
		public string? FullName { get; set; }

		[Display(Name = "DisplayName")]
		[StringLength(30, ErrorMessage = "Tên hiển thị không được vượt quá 30 ký tự")]
		[MinLength(3, ErrorMessage = "Tên hiển thị phải có ít nhất 3 ký tự")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên hiển thị chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
		public string? DisplayName { get; set; }

		[Display(Name = "Số điện thoại")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải có độ dài từ 9 đến 11 ký tự và chỉ chứa các chữ số.")]
		public string? PhoneNumber { get; set; }
		public string? Gender { get; set; }
		public DateOnly? Birthdate { get; set; }
		public IFormFile? Image { get; set; }
	}
}
