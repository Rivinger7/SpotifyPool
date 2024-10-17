using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView
{
	public class ResetPasswordRequestModel
	{

        [Display(Name = "Mật khẩu mới")]
        [Required(ErrorMessage = "* Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string NewPassword { get; set; }


        [Display(Name = "Xác Nhận Mật khẩu mới")]
        [Required(ErrorMessage = "* Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string ConfirmPassword { get; set; }
    }
}
