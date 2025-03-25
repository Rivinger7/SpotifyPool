namespace BusinessLogicLayer.ModelView.Service_Model_Views.Dashboard.Response
{
	public class UserGrowthDashboard
	{
		public string Month { get; set; } = null!;

		public int NewUsers { get; set; }

		public int ActiveUsers { get; set; }
	}
}
