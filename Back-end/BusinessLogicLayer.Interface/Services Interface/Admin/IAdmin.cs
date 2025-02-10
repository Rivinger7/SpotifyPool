using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;

namespace BusinessLogicLayer.Interface.Services_Interface.Admin
{
	public interface IAdmin
	{
		Task<IEnumerable<AdminResponse>> GetAllAccountAsync(PagingRequestModel request, AdminFilter model);
		Task<AdminDetailResponse> GetByIdAsync(string id);

		Task UpdateByIdAsync(string id, UpdateUserRequest userRequest);

		Task DeleteByIdAsync(string id);
	}
}
