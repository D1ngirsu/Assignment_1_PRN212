using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Repositories
{
    public interface ISystemAccountRepository : IGenericRepository<SystemAccount>
    {
        Task<SystemAccount?> GetByEmailAsync(string email);
        Task<SystemAccount?> GetByEmailAndPasswordAsync(string email, string password);
        Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<SystemAccount>> GetAccountsWithNewsArticlesAsync();
    }
}
