using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;

namespace SpotifyPool.GraphQL.Authentication
{
    public class AuthenticationMutation(IAuthentication authenticationService)
    {
        private readonly IAuthentication _authenticationService = authenticationService;

        public async Task<AuthenticatedUserInfoResponseModel> Login(LoginRequestModel loginModel)
        {
            AuthenticatedUserInfoResponseModel accessToken = await _authenticationService.Authenticate(loginModel);
            return accessToken;
        }
    }
}
