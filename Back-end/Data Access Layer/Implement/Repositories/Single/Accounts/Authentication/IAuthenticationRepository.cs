﻿using Business_Logic_Layer.Models;
using Data_Access_Layer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Implement.Repositories.Single.Accounts.Authentication
{
    public interface IAuthenticationRepository
    {
        Task<User> Authenticate(User user);
        Task CreateAccount(User user);
        Task CheckAccountExists(User user);
        Task<string> GetPasswordHashed(string username);
        Task UpdateTokenByEmail(string email, string token);
        Task UpdateAccountConfirmationStatus(string? email);
        Task<string> GetAccountToken(string? email);
    }
}