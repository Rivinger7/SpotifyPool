namespace BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response
{
    public class AuthenticatedUserInfoResponseModel
    {
        public string? Id { get; set; }
        public List<string>? Role { get; set; } = [];
        public string? Name { get; set; }
        public List<string>? Avatar { get; set; } = [];
    }
}
