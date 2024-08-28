using Business_Logic_Layer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business_Logic_Layer.Services.JWT
{
    public class JWT
	{
		private readonly IConfiguration _config;

        public JWT(IConfiguration config)
		{
			_config = config;
		}
		public string GenerateJWTToken(CustomerModel customerModel)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();

			var secretKeyBytes = Encoding.UTF8.GetBytes(_config["JWTSettings:SecretKey"]);

			var tokenDescription = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] {
							new Claim("Username", customerModel.Username),
                            new Claim("Email", customerModel.Email),
							new Claim(ClaimTypes.Role, customerModel.Role),
				}),
				Expires = DateTime.UtcNow.AddHours(24),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = jwtTokenHandler.CreateToken(tokenDescription);

			return jwtTokenHandler.WriteToken(token);
		}

        public string GenerateJWTTokenForConfirmEmail(string email, string encrpytedToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_config["JWTSettings:SecretKey"]);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                            new Claim("Email", email),
                            new Claim(ClaimTypes.Role, "Customer"),
                            new Claim("EncrpytedToken", encrpytedToken)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);

            return jwtTokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_config["JWTSettings:SecretKey"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, // Nếu muốn kiểm tra Issuer, đặt thành true và cấu hình Issuer ở đây
                ValidateAudience = false, // Nếu muốn kiểm tra Audience, đặt thành true và cấu hình Audience ở đây
                ValidateLifetime = true, // Kiểm tra token có hết hạn hay không
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ClockSkew = TimeSpan.Zero // Không có độ trễ khi xác thực thời gian hết hạn
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Kiểm tra nếu token không hợp lệ hoặc có vấn đề khác
                if (validatedToken == null)
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenException ex)
            {
                // Token không hợp lệ hoặc đã hết hạn
                Console.WriteLine(ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public JwtSecurityToken DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Giải mã token JWT mà không cần xác thực
            var decodedToken = tokenHandler.ReadJwtToken(token);
            return decodedToken;
        }
    }
}
