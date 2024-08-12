using Data_Access_Layer.DBContext;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Repositories.Accounts.Authentication
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
                    Username = user.Username,
                    Password = user.Password,
                    Email = user.Email,
                    Phonenumber = user.Phonenumber,
                    Role = "Customer",
                    Status = "Inactive"
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
            string username = user.Username;
            string email = user.Email;
            string phonenumber = user.Phonenumber;

            bool usernameExists = await _context.Users.Find(user => user.Username == username).AnyAsync();
            if (usernameExists)
            {
                throw new ArgumentException("Username already exists", "usernameExists");
            }

            bool emailExists = await _context.Users.Find(user => user.Email == email).AnyAsync();
            if (emailExists)
            {
                throw new ArgumentException("Email already exists", "emailExists");
            }

            bool phonenumberExists = await _context.Users.Find(user => user.Phonenumber == phonenumber).AnyAsync();
            if (phonenumberExists)
            {
                throw new ArgumentException("Phone number already exists", "phoneNumberExists");
            }
        }

        public async Task<User> Authenticate(User user)
        {
            string username = user.Username;
            string password = user.Password;

            User retrieveUser = await _context.Users.Find(user => user.Username == username && user.Password == password).FirstOrDefaultAsync();

            if (retrieveUser is null)
            {
                throw new ArgumentException("Username or Password is incorrect", "loginFail");
            }

            // Role thì bên FE sẽ tự validation. Vì mỗi User sẽ có role khác nhau

            // Remember me cũng do bên FE quản lý luôn

            //string status = retrieveUser.Status.ToUpper();
            //switch(status)
            //{
            //    case "BANNED":
            //        throw new ArgumentException("Account has been locked", "bannedStatus");
            //    case "INACTIVE":
            //        throw new ArgumentException("Account not activated", "inactiveStatus");
            //}


            return retrieveUser;
        }
    }
}
