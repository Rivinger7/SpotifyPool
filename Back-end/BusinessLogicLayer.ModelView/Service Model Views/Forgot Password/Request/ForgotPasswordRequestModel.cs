using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request
{
	public class ForgotPasswordRequestModel
	{
        [Required(ErrorMessage = "The email can not be empty!")]
        [EmailAddress(ErrorMessage = "The email does not have right format")]
        public string Email { get; set; } = null!;

    }
}
