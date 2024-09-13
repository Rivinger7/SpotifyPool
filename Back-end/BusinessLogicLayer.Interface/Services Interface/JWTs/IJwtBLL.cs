using BusinessLogicLayer.ModelView.Service_Model_Views.JWTs.Request;
using DataAccessLayer.Repository.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BusinessLogicLayer.Interface.Services_Interface.JWTs
{
    public interface IJwtBLL
    {
        void GenerateAccessToken(IEnumerable<Claim> claims, User user, out string accessToken, out string refreshToken);
        void RevokeToken(string Id);
        void RefreshAccessToken(out string newAccessToken, out string newRefreshToken, TokenApiRequestModel tokenApiModel);
        string GenerateJWTTokenForConfirmEmail(string email, string encrpytedToken);
        JwtSecurityToken DecodeToken(string token);
    }
}
