using Data_Access_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Implement.Repositories.Single.Accounts.Customers
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetByUsername(string? username);
    }
}
