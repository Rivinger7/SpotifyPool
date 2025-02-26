using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Paging
{
	public class PagingRequestModel
	{
		[Range(1, int.MaxValue, ErrorMessage = "Number of pages must be greater than 1")]
		public int PageNumber { get; set; } = 1;

		[Range(1, int.MaxValue, ErrorMessage = "Page capacity must be greater than 1")]
		public int PageSize { get; set; } = 5;

	}
}
