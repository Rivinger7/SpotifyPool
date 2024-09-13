using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;

namespace Business_Logic_Layer.Services_Interface.Users
{
    public interface IUserBLL
    {
        // Get User List
        Task<IEnumerable<UserResponseModel>> GetAllUsersAsync(string? fullname, string? gender, string? email, bool isCache = false);
        // Get user by ID
        Task<UserResponseModel> GetUserByIDAsync(string id, bool isCache = false);
    }
}
