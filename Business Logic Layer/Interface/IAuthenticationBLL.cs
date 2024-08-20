using Business_Logic_Layer.Models;

namespace Business_Logic_Layer.Interface
{
    public interface IAuthenticationBLL
    {
        Task CreateAccount(RegisterModel registerModel);
        Task ActivateAccountByToken(string token);
        Task<CustomerModel> Authenticate(LoginModel loginModel);
        Task ReActiveAccountByToken(string username);


    }
}
