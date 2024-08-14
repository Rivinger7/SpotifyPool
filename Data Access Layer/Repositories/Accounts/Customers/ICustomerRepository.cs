using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Repositories.Accounts.Customers
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetByUsername(string? username);
    }
}
