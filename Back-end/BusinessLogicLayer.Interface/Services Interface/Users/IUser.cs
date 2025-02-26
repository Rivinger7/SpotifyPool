using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using SetupLayer.Enum.Services.User;

namespace Business_Logic_Layer.Services_Interface.Users
{
    public interface IUser
    {
        // Get User List
        Task<IEnumerable<UserResponseModel>> GetAllUsersAsync(string? fullname, UserGender? gender, string? email, bool isCache = false);
        Task<UserProfileResponseModel> GetUserByIDAsync(string id);
        Task EditProfileAsync(EditProfileRequestModel requestModel);
		Task<IEnumerable<UserResponseModel>> TestPaging(int offset, int limit);
        Task<AuthenticatedResponseModel> SwitchToArtistProfile();
    }
}
