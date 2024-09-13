using BusinessLogicLayer.ModelView;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;

namespace BusinessLogicLayer.Interface.Services_Interface.Authentication
{
    public interface IAuthenticationBLL
    {
        Task CreateAccount(RegisterRequestModel registerModel);
        Task ActivateAccountByToken(string token);
        Task<AuthenticatedResponseModel> Authenticate(LoginRequestModel loginModel);
        Task ReActiveAccountByToken(string username);
        Task<AuthenticatedResponseModel> LoginByGoogle();
        Task<AuthenticatedResponseModel> ConfirmLinkWithGoogleAccount(string email);
        Task<string> SendTokenForgotPasswordAsync(ForgotPasswordRequestModel model);
        Task ResetPasswordAsync(ResetPasswordRequestModel model);
    }
}
