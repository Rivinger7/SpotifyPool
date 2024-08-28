using BusinessLogicLayer.ModelView.Models;
using Data_Access_Layer.Entities;
using System.Security.Claims;

namespace Business_Logic_Layer.Interface
{
    public interface IJwtBLL
    {
        void GenerateAccessToken(IEnumerable<Claim> claims, User user, out string accessToken, out string refreshToken);
        void RevokeToken(string Id);
        void RefreshAccessToken(out string newAccessToken, out string newRefreshToken, TokenApiModel tokenApiModel);
    }
}
