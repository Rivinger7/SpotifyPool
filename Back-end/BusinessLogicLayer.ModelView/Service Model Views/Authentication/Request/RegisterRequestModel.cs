using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request
{
    public class RegisterRequestModel
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "* Vui lòng nhập tên đăng nhập")]
        [StringLength(30, ErrorMessage = "Tên đăng nhập không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên đăng nhập chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "* Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string Password { get; set; }

        [Display(Name = "Xác Nhận Mật khẩu")]
        [Required(ErrorMessage = "* Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string ConfirmedPassword { get; set; }

        [Display(Name = "Tên hiển thị")]
        [Required(ErrorMessage = "* Vui lòng nhập tên hiển thị")]
        [StringLength(30, ErrorMessage = "Tên hiển thị không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Tên hiển thị phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên hiển thị chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string DisplayName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "* Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [MinLength(3, ErrorMessage = "Email phải có ít nhất 3 ký tự")]
        [StringLength(50, ErrorMessage = "Email không được vượt quá 50 ký tự")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "* Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải có độ dài từ 9 đến 11 ký tự và chỉ chứa các chữ số.")]
        public string PhoneNumber { get; set; }
    }
}
