﻿using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView.Service_Model_Views.JWTs.Request;
using DataAccessLayer.Repository.Database_Context.MongoDB.SpotifyPool;
using DataAccessLayer.Repository.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.JWTs
{
    public class JwtBLL(SpotifyPoolDBContext context) : IJwtBLL
    {
        private readonly SpotifyPoolDBContext _context = context;

        /// <summary>
        /// Generate access token with claims (user's informations)
        /// </summary>
        /// <param name="claims">A list of Claim having user information</param>
        /// <returns>A string of token which generated by Claim list</returns>
        private static string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            int expireMinutes = 60; //set default expire time is 60 minutes

            //get secret key from appsettings.json
            var secretKey = Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found");

            //convert secret key to byte array
            var symmetricKey = Encoding.UTF8.GetBytes(secretKey);

            //create token with JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new JwtSecurityToken(
                issuer: "https://localhost:7018/", //set issuer is localhost

                audience: "https://localhost:7018/", //set audience is localhost

                claims: claims,

                expires: Util.GetUtcPlus7Time().Add(TimeSpan.FromMinutes(expireMinutes)),

                signingCredentials: new SigningCredentials(
                                    new SymmetricSecurityKey(symmetricKey),
                                    SecurityAlgorithms.HmacSha256Signature) //use HmacSha256Signature algorithm to sign token
            );
            //write token with tokenDescriptor above
            var token = tokenHandler.WriteToken(tokenDescriptor);
            return token;
        }

        /// <summary>
        /// Refresh access token for expired token
        /// </summary>
        /// <returns></returns>
        private static string GenerateRefreshToken()
        {
            //generate random number for refresh token
            var randomNumber = new byte[32];

            //use RandomNumberGenerator to create random number
            using var rng = RandomNumberGenerator.Create();
            //get random number and convert to base64 string
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Method get all Claim with related user 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="SecurityTokenException"></exception>
        private static ClaimsPrincipal GetPrincipalFromExpiredToken(string? token)
        {
            //set token validation parameters
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateAudience = false,

                ValidateIssuer = false,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found"))), //Sign with encoded secret key

                ValidateLifetime = false //this field not need to check validate because we just want to get principal from that token
            };

            //get principal from token from tokenValidationParameters (information Claim in here)
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            //check if token is null or not and compare algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
            {
                //throw exception if information in token is invalid
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }


        /// <summary>
        /// Generate token Method in Service for API
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="Id"></param>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        public void GenerateAccessToken(IEnumerable<Claim> claims, User user, out string accessToken, out string refreshToken)
        {
            ObjectId userID = user.Id;

            //generate access token and refresh token
            accessToken = GenerateAccessToken(claims);

            refreshToken = GenerateRefreshToken();

            //set refresh token, expirytime for refreshtoken to user and save to database
            UpdateDefinition<User> refeshTokenUpdate = Builders<User>.Update.Set(user => user.RefreshToken, refreshToken);

            DateTime refreshTokenExpiryTime = Util.GetUtcPlus7Time().AddDays(5);
            UpdateDefinition<User> refreshTokenExpiryTimeUpdate = Builders<User>.Update.Set(user => user.RefreshTokenExpiryTime, refreshTokenExpiryTime);

            // Update
            UpdateResult refeshTokenUpdateResult = _context.Users.UpdateOne(user => user.Id == userID, refeshTokenUpdate);
            UpdateResult refreshTokenExpiryTimeUpdateResult = _context.Users.UpdateOne(user => user.Id == userID, refreshTokenExpiryTimeUpdate);

            if (refeshTokenUpdateResult.ModifiedCount < 1 || refreshTokenExpiryTimeUpdateResult.ModifiedCount < 1)
            {
                throw new ArgumentException("Found User but can not update");
            }

            return;
        }

        /// <summary>
        /// Revoke token Method in Service for API
        /// </summary>
        /// <param name="Id"></param>
        /// <exception cref="ErrorException"></exception>
        public async void RevokeToken(string Id)
        {
            // Retrieve an user from the database
            User retrieveUser = await _context.Users.Find(user => user.Id.ToString() == Id).FirstOrDefaultAsync() ?? throw new ArgumentException("Not found any available user");

            // Update
            UpdateDefinition<User> refeshTokenUpdate = Builders<User>.Update.Set(user => user.RefreshToken, null);
            UpdateResult refeshTokenUpdateResult = await _context.Users.UpdateOneAsync(user => user.Id.ToString() == Id, refeshTokenUpdate);

            return;
        }


        /// <summary>
        /// Refresh token Method in Service for API
        /// </summary>
        /// <param name="newAccessToken"></param>
        /// <param name="newRefreshToken"></param>
        /// <param name="tokenApiModel"></param>
        /// <exception cref="ErrorException"></exception>
        public void RefreshAccessToken(out string newAccessToken, out string newRefreshToken, TokenApiRequestModel tokenApiModel)
        {
            //Check if tokenApiModel is null or not
            tokenApiModel = tokenApiModel ?? throw new ArgumentException("Please fill all of information");

            string? accessToken = tokenApiModel.AccessToken;
            string? refreshToken = tokenApiModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(tokenApiModel.AccessToken);

            string userIDString = principal.Identity?.Name ?? throw new ArgumentException("User's ID is not found in any session"); //this is mapped to the Name claim by default

            ObjectId userID = ObjectId.Parse(userIDString);

            User retrieveUser = _context.Users.Find(user => user.Id == userID).FirstOrDefault() ?? throw new ArgumentException("User's ID is not found");

            //check valid for refresh token and expiry time
            if (retrieveUser.RefreshToken == null || retrieveUser.RefreshToken != refreshToken || retrieveUser.RefreshTokenExpiryTime <= Util.GetUtcPlus7Time())
            {
                throw new ArgumentException("Refresh token is incorrect or user is invalid");
            }

            //generate new access token and refresh token
            newAccessToken = GenerateAccessToken(principal.Claims);
            newRefreshToken = GenerateRefreshToken();

            UpdateDefinition<User> refeshTokenUpdate = Builders<User>.Update.Set(user => user.RefreshToken, newRefreshToken);
            UpdateResult refeshTokenUpdateResult = _context.Users.UpdateOne(user => user.Id == userID, refeshTokenUpdate);
        }

        public string GenerateJWTTokenForConfirmEmail(string email, string encrpytedToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new DataNotFoundCustomException("JWT's Secret Key property is not set in environment or not found"));

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

        public JwtSecurityToken DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Giải mã token JWT mà không cần xác thực
            var decodedToken = tokenHandler.ReadJwtToken(token);
            return decodedToken;
        }
    }
}