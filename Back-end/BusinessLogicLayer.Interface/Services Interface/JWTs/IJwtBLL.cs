using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BusinessLogicLayer.Interface.Services_Interface.JWTs
{
    public interface IJwtBLL
    {
        Task RevokeToken(string Id);
        void RefreshAccessToken(out string newAccessToken, out string newRefreshToken, out ClaimsPrincipal principal, string oldRefreshToken);
        string GenerateJWTTokenForConfirmEmail(string email, string encrpytedToken);
        JwtSecurityToken DecodeToken(string token);
        void GenerateAccessToken(IEnumerable<Claim> claims, string userId, out string accessToken, out string refreshToken);
        ClaimsPrincipal ValidateToken(string token);
        void Dispose();
    }
}
