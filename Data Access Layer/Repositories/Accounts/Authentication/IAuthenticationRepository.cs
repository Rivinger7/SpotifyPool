using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Repositories.Accounts.Authentication
{
    public interface IAuthenticationRepository
    {
        Task<User> Authenticate(User user);
        Task CreateAccount(User user);
        Task CheckAccountExists(User user);
    }
}
