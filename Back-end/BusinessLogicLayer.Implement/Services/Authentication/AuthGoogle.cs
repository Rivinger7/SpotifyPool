using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Authentication;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Authentication
{
    public class AuthGoogle(IJwtBLL jwtBLL) : IAuthGoogle
    {
        private readonly IJwtBLL _jwtBLL = jwtBLL;
        public async Task<string> AuthenticateGoogleUserAsync(string googleToken)
        {
            // Xác thực token của Google và lấy thông tin người dùng
            var payload = await VerifyGoogleToken(googleToken) ?? throw new UnauthorizedAccessException("Invalid Google token.");

            // Phát JWT token
            var token = GenerateJwtToken(payload);
            return token;
        }

        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string googleToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [Environment.GetEnvironmentVariable("Authentication_Google_ClientId")]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
            return payload;
        }

        private static string GenerateJwtToken(GoogleJsonWebSignature.Payload payload)
        {
            Claim[] claims =
            [
            new Claim(JwtRegisteredClaimNames.Sub, payload.Subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, payload.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, payload.GivenName),
            new Claim(JwtRegisteredClaimNames.FamilyName, payload.FamilyName),
            new Claim(JwtRegisteredClaimNames.Name, payload.Name),
            new Claim(JwtRegisteredClaimNames.Picture, payload.Picture),
            ];

            string email = payload.Email;

            //get secret key from appsettings.json
            var secretKey = Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found");

            //convert secret key to byte array
            var symmetricKey = Encoding.UTF8.GetBytes(secretKey);

            int expireMinutes = 60;

            var token = new JwtSecurityToken(
               issuer: "https://localhost:7018/", //set issuer is localhost

                audience: "https://localhost:7018/", //set audience is localhost

                claims: claims,

                expires: Util.GetUtcPlus7Time().Add(TimeSpan.FromMinutes(expireMinutes)),

                signingCredentials: new SigningCredentials(
                                    new SymmetricSecurityKey(symmetricKey),
                                    SecurityAlgorithms.HmacSha256Signature) //use HmacSha256Signature algorithm to sign token
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
