using BusinessObjects.Models;
using DataAccessObject;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessObjects
{
    public class SystemAccountDAO : GenericRepository<SystemAccount>, ISystemAccountRepository
    {
        public SystemAccountDAO(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<SystemAccount?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.AccountEmail.ToLower() == email.ToLower());
        }

        public async Task<SystemAccount?> GetByEmailAndPasswordAsync(string email, string password)
        {
            return await _dbSet.FirstOrDefaultAsync(a =>
                a.AccountEmail.ToLower() == email.ToLower() &&
                a.AccountPassword == password);
        }

        public async Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role)
        {
            return await _dbSet
                .Where(a => a.AccountRole == role)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(a => a.AccountEmail.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<SystemAccount>> GetAccountsWithNewsArticlesAsync()
        {
            return await _context.SystemAccounts
                .Include(a => a.NewsArticles)
                .Where(a => a.NewsArticles.Any())
                .OrderBy(a => a.AccountName)
                .ToListAsync();
        }
    }
}