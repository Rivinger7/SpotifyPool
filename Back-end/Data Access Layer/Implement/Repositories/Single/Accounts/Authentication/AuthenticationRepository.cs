using Data_Access_Layer.DBContext;
using Data_Access_Layer.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Implement.Repositories.Single.Accounts.Authentication
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly SpotifyPoolDBContext _context;

        public AuthenticationRepository(SpotifyPoolDBContext context)
        {
            _context = context;
        }

        public async Task CreateAccount(User user)
        {
            try
            {
                User newUser = new()
                {
                    UserName = user.UserName,
                    //Password = user.Password,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = "Customer",
                    Status = "Inactive",
                    TokenEmailConfirm = user.TokenEmailConfirm
                };

                await _context.Users.InsertOneAsync(newUser);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task CheckAccountExists(User user)
        {
            string UserName = user.UserName;
            string email = user.Email;
            string phonenumber = user.PhoneNumber;

            bool UserNameExists = await _context.Users.Find(user => user.UserName == UserName).AnyAsync();
            if (UserNameExists)
            {
                throw new ArgumentException("UserName already exists", "UserNameExists");
            }

            bool emailExists = await _context.Users.Find(user => user.Email == email).AnyAsync();
            if (emailExists)
            {
                throw new ArgumentException("Email already exists", "emailExists");
            }

            bool phonenumberExists = await _context.Users.Find(user => user.PhoneNumber == phonenumber).AnyAsync();
            if (phonenumberExists)
            {
                throw new ArgumentException("Phone number already exists", "phoneNumberExists");
            }
        }

        public async Task<string> GetAccountToken(string? email)
        {
            try
            {
                string? token = await _context.Users.Find(user => user.Email == email).Project(user => user.TokenEmailConfirm)
                    .FirstOrDefaultAsync();

                if (token is null)
                {
                    throw new ArgumentException("The account has activated", "activatedAccount");
                }

                return token;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateTokenByEmail(string email, string token)
        {
            try
            {
                User retrieveUser = await _context.Users.Find(user => user.Email == email).FirstOrDefaultAsync();

                UpdateDefinition<User> tokenUpdate = Builders<User>.Update.Set(user => user.TokenEmailConfirm, token);
                UpdateResult tokenResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, tokenUpdate);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateAccountConfirmationStatus(string? email)
        {
            try
            {
                User retrieveUser = await _context.Users.Find(user => user.Email == email).FirstOrDefaultAsync();

                if (retrieveUser is null)
                {
                    throw new ArgumentException("Not found any user", "notFound");
                }

                UpdateDefinition<User> statusUpdate = Builders<User>.Update.Set(user => user.Status, "Active");

                UpdateDefinition<User> tokenUpdate = Builders<User>.Update.Set(user => user.TokenEmailConfirm, null);

                UpdateResult statusResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, statusUpdate);

                UpdateResult tokenResult = await _context.Users.UpdateOneAsync(user => user.Id == retrieveUser.Id, tokenUpdate);

                // Kiểm tra nếu có ít nhất một field_name được cập nhật
                if (statusResult.ModifiedCount < 1)
                {
                    throw new ArgumentException("Found User but can not update", "updateFail");
                }

                return;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

		//EDITED Password
		public async Task<User> Authenticate(User user)
        {
            string UserName = user.UserName;
            string password = user.PasswordHash;

            User retrieveUser = await _context.Users.Find(user => user.UserName == UserName && user.PasswordHash == password).FirstOrDefaultAsync();

            if (retrieveUser is null)
            {
                throw new ArgumentException("UserName or Password is incorrect", "loginFail");
            }

            // Role thì bên FE sẽ tự validation. Vì mỗi User sẽ có role khác nhau

            // Remember me cũng do bên FE quản lý luôn

            string status = retrieveUser.Status.ToUpper();
            switch (status)
            {
                case "BANNED":
                    throw new ArgumentException("Account has been locked", "bannedStatus");
                case "INACTIVE":
                    throw new ArgumentException("Account is not activated", "inactiveStatus");
            }

            return retrieveUser;
        }

        //EDITED Password
        public async Task<string> GetPasswordHashed(string UserName)
        {
            string passwordHashed = await _context.Users.Find(user => user.UserName == UserName).Project(user => user.PasswordHash)
                .FirstOrDefaultAsync();

            return passwordHashed;
        }
    }
}
