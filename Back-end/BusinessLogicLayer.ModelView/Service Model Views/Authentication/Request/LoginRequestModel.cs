using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request
{
    public class LoginRequestModel
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "* Vui lòng nhập tên đăng nhập")]
        [StringLength(30, ErrorMessage = "Tên đăng nhập không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên đăng nhập chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string Username { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "* Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
