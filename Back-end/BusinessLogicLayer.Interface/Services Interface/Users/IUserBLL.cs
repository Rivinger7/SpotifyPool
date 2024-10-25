using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using Utility.Coding;

namespace Business_Logic_Layer.Services_Interface.Users
{
    public interface IUserBLL
    {
        // Get User List
        Task<IEnumerable<UserResponseModel>> GetAllUsersAsync(string? fullname, string? gender, string? email, bool isCache = false);
        // Get user by ID
        Task<UserResponseModel> GetUserByIDAsync(string id, bool isCache = false);

        Task EditProfileAsync(EditProfileRequestModel requestModel);

        Task<BasePaginatedList<BsonDocument>> Test(int index, int page);

	}
}
