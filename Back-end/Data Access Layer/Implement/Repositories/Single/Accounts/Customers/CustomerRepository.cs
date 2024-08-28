using Data_Access_Layer.DBContext;
using Data_Access_Layer.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Implement.Repositories.Single.Accounts.Customers
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly SpotifyPoolDBContext _context;

        public CustomerRepository(SpotifyPoolDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            IEnumerable<User> users = await _context.Users.Find(users => true).ToListAsync();
            return users;
        }

        public async Task<User> GetByUsername(string? username)
        {
            try
            {
                User user = await _context.Users.Find(user => username == user.Username).FirstOrDefaultAsync();
                if (user is null)
                {
                    throw new ArgumentException("The Account does not exists", "accountNotFound");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }
    }
}
