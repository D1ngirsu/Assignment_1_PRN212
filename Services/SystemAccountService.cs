using BusinessObjects.Models;
using Repositories;
using Services;

namespace Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _systemAccountRepository;

        public SystemAccountService(ISystemAccountRepository systemAccountRepository)
        {
            _systemAccountRepository = systemAccountRepository;
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
            // Business validation
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (string.IsNullOrWhiteSpace(account.AccountEmail))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(account.AccountPassword))
                throw new ArgumentException("Password is required");

            if (string.IsNullOrWhiteSpace(account.AccountName))
                throw new ArgumentException("Account name is required");

            // Check if email already exists
            if (await _systemAccountRepository.EmailExistsAsync(account.AccountEmail))
                throw new InvalidOperationException($"Email {account.AccountEmail} already exists");

            // Set default values
            account.AccountRole = account.AccountRole ?? 0; // Default role, e.g., user

            return await _systemAccountRepository.AddAsync(account);
        }

        public async Task UpdateAccountAsync(SystemAccount account)
        {
            // Business validation
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            var existing = await _systemAccountRepository.GetByIdAsync(account.AccountId);
            if (existing == null)
                throw new KeyNotFoundException($"Account with ID {account.AccountId} not found");

            if (string.IsNullOrWhiteSpace(account.AccountEmail))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(account.AccountPassword))
                throw new ArgumentException("Password is required");

            if (string.IsNullOrWhiteSpace(account.AccountName))
                throw new ArgumentException("Account name is required");

            // Check if email changed and exists
            if (account.AccountEmail != existing.AccountEmail && await _systemAccountRepository.EmailExistsAsync(account.AccountEmail))
                throw new InvalidOperationException($"Email {account.AccountEmail} already exists");

            await _systemAccountRepository.UpdateAsync(account);
        }

        public async Task DeleteAccountAsync(short accountId)
        {
            var account = await _systemAccountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {accountId} not found");

            // Optionally check if account has news articles
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

            account.AccountPassword = newPassword;

            await _systemAccountRepository.UpdateAsync(account);
        }
    }
}