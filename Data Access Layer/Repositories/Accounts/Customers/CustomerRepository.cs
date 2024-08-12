using Data_Access_Layer.DBContext;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Repositories.Accounts.Customers
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
            if (users is null)
            {
                return null;
            }
            return users;
        }
    }
}
