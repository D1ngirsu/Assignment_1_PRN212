using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISystemAccountService
    {
        Task<IEnumerable<SystemAccount>> GetAllAccountsAsync();
        Task<SystemAccount?> GetAccountByIdAsync(short accountId);
        Task<SystemAccount?> GetAccountByEmailAsync(string email);
        Task<SystemAccount?> LoginAsync(string email, string password);
        Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role);
        Task<SystemAccount> CreateAccountAsync(SystemAccount account);
        Task UpdateAccountAsync(SystemAccount account);
        Task DeleteAccountAsync(short accountId);
        Task<bool> EmailExistsAsync(string email);
        Task ChangePasswordAsync(short accountId, string newPassword);
    }

}
