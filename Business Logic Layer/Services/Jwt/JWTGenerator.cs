using Business_Logic_Layer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services.JWT
{
	public class JWTGenerator
	{
		private readonly IConfiguration _config;
		public JWTGenerator(IConfiguration config)
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
							new Claim(ClaimTypes.Role, customerModel.Role),
				}),
				Expires = DateTime.UtcNow.AddHours(24),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = jwtTokenHandler.CreateToken(tokenDescription);

			return jwtTokenHandler.WriteToken(token);
		}
	}
}
