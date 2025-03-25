using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request
{
	public class EditProfileRequestModel
	{
		[Display(Name = "DisplayName")]
		[Required(ErrorMessage = "Tên hiển thị không được để trống")]
        [StringLength(30, ErrorMessage = "Tên hiển thị không được vượt quá 30 ký tự")]
		public string DisplayName { get; set; }

		public IFormFile? Image { get; set; }
	}
}
