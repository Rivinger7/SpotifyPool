using BusinessLogicLayer.ModelView;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Forgot_Password.Request;

namespace BusinessLogicLayer.Interface.Services_Interface.Authentication
{
    public interface IAuthentication
    {
        Task CreateAccount(RegisterRequestModel registerModel);
        Task ActivateAccountByToken(string token);
        Task<AuthenticatedUserInfoResponseModel> Authenticate(LoginRequestModel loginModel);
        Task ReActiveAccountByToken();
        Task<AuthenticatedResponseModel> LoginByGoogle(string googleToken);
        Task<AuthenticatedResponseModel> ConfirmLinkWithGoogleAccount(string email);
        Task SendOTPForgotPasswordAsync(ForgotPasswordRequestModel model);
        Task ConfirmOTP(string userId, string otpCode);
        Task ResetPasswordAsync(ResetPasswordRequestModel model);
        Task<AuthenticatedResponseModel> SwitchProfile();
        AuthenticatedUserInfoResponseModel GetUserInformation(string token);
        public AuthenticatedUserInfoResponseModel Relog();
    }
}
