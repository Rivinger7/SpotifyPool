using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Models
{
	public class ForgotPasswordModel
	{
		[Required(ErrorMessage = "The email can not be empty!")]
		[EmailAddress(ErrorMessage = "The email does not have right format")]
		public string? Email { get; set; }

		[Required]
		public string? ClientUrl { get; set; }
	}
}
