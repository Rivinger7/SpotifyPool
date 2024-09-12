using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView
{
	public class ResetPasswordRequestModel
	{
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Display(Name = "Xác Nhận Mật khẩu")]
        [Required(ErrorMessage = "* Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }
    }
}
