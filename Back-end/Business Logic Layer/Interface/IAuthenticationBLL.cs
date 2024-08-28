using Business_Logic_Layer.Models;
using BusinessLogicLayer.ModelView.Models;

namespace Business_Logic_Layer.Interface
{
    public interface IAuthenticationBLL
    {
        Task CreateAccount(RegisterModel registerModel);
        Task ActivateAccountByToken(string token);
        Task<AuthenticatedResponseModel> Authenticate(LoginModel loginModel);
        Task ReActiveAccountByToken(string username);
        Task<AuthenticatedResponseModel> LoginByGoogle();

    }
}
