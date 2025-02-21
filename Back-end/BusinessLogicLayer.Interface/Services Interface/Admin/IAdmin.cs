using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;

namespace BusinessLogicLayer.Interface.Services_Interface.Admin
{
	public interface IAdmin
	{
		Task<IEnumerable<AccountResponse>> GetPaggingAsync(PagingRequestModel request, AccountFilterModel model);

		Task<AccountDetailResponse> GetByIdAsync(string id);

		Task CreateAsync(CreateRequestModel accountRequest);

		Task UpdateByIdAsync(string id, UpdateRequestModel accountRequest);

		Task DeleteByIdAsync(string id);
	}
}
