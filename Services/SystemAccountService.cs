using BusinessObjects.Models;
using Repositories;
using Services;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _systemAccountRepository;
        private readonly IConfiguration _configuration;

        public SystemAccountService(ISystemAccountRepository systemAccountRepository, IConfiguration configuration)
        {
            _systemAccountRepository = systemAccountRepository;
            _configuration = configuration;
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAccountsAsync()
        {
            return await _systemAccountRepository.GetAllAsync();
        }

        public async Task<SystemAccount?> GetAccountByIdAsync(short accountId)
        {
            return await _systemAccountRepository.GetByIdAsync(accountId);
        }

        public async Task<SystemAccount?> GetAccountByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _systemAccountRepository.GetByEmailAsync(email);
        }

        public async Task<SystemAccount?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            return await _systemAccountRepository.GetByEmailAndPasswordAsync(email, password);
        }

        public async Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role)
        {
            return await _systemAccountRepository.GetAccountsByRoleAsync(role);
        }

        public async Task<SystemAccount> CreateAccountAsync(SystemAccount account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (string.IsNullOrWhiteSpace(account.AccountEmail))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(account.AccountPassword))
                throw new ArgumentException("Password is required");

            if (string.IsNullOrWhiteSpace(account.AccountName))
                throw new ArgumentException("Account name is required");

            if (await _systemAccountRepository.EmailExistsAsync(account.AccountEmail))
                throw new InvalidOperationException($"Email {account.AccountEmail} already exists");

            account.AccountPassword = PasswordHelper.HashPassword(account.AccountPassword);
            account.AccountRole = account.AccountRole ?? UserRoles.Lecturer;

            return await _systemAccountRepository.AddAsync(account);
        }

        public async Task UpdateAccountAsync(SystemAccount account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            var existing = await _systemAccountRepository.GetByIdAsync(account.AccountId);
            if (existing == null)
                throw new KeyNotFoundException($"Account with ID {account.AccountId} not found");

            if (string.IsNullOrWhiteSpace(account.AccountEmail))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(account.AccountName))
                throw new ArgumentException("Account name is required");

            if (!string.IsNullOrWhiteSpace(account.AccountPassword))
            {
                account.AccountPassword = PasswordHelper.HashPassword(account.AccountPassword);
            }
            else
            {
                account.AccountPassword = existing.AccountPassword;
            }

            if (account.AccountEmail != existing.AccountEmail && await _systemAccountRepository.EmailExistsAsync(account.AccountEmail))
                throw new InvalidOperationException($"Email {account.AccountEmail} already exists");

            await _systemAccountRepository.UpdateAsync(account);
        }

        public async Task DeleteAccountAsync(short accountId)
        {
            var account = await _systemAccountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {accountId} not found");

            var accountsWithNews = await _systemAccountRepository.GetAccountsWithNewsArticlesAsync();
            if (accountsWithNews.Any(a => a.AccountId == accountId))
                throw new InvalidOperationException($"Cannot delete account {accountId} as it has associated news articles");

            await _systemAccountRepository.DeleteAsync(accountId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _systemAccountRepository.EmailExistsAsync(email);
        }

        public async Task ChangePasswordAsync(short accountId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password is required");

            var account = await _systemAccountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {accountId} not found");

            account.AccountPassword = PasswordHelper.HashPassword(newPassword);
            await _systemAccountRepository.UpdateAsync(account);
        }

        public async Task EnsureDefaultAdminAsync()
        {
            var defaultEmail = _configuration["DefaultAdmin:Email"];
            if (string.IsNullOrEmpty(defaultEmail))
                return;

            if (await EmailExistsAsync(defaultEmail))
                return;

            var defaultPass = _configuration["DefaultAdmin:Password"];
            if (string.IsNullOrEmpty(defaultPass))
                return;

            var hashedPass = PasswordHelper.HashPassword(defaultPass);
            var adminRole = _configuration.GetValue<int>("Roles:Admin", UserRoles.Admin);

            var adminAccount = new SystemAccount
            {
                AccountName = "System Administrator",
                AccountEmail = defaultEmail,
                AccountPassword = hashedPass,
                AccountRole = adminRole
            };

            await CreateAccountAsync(adminAccount);
        }
    }
}
