using BusinessLogicLayer.ModelView.Service_Model_Views.Dashboard.Response;

namespace BusinessLogicLayer.Interface.Services_Interface.Dashboard
{
	public interface IDashboard
	{
		Task<SystemOverviewResponse> GetSystemOverviewAsync();

		Task<DashboardTrackArtistManagemen> GetTrackArtistManagementAsync();

		Task<List<UserGrowthDashboard>> GetUserGrowthAsync();

		Task<List<RoleDistributionDashboard>> GetUserRoleDistributionAsync();

	}
}
