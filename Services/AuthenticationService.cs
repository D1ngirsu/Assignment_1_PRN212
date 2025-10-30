using BusinessObjects.Models;
using Services;
using BCrypt.Net;

namespace Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ISystemAccountService _accountService;

        public AuthenticationService(ISystemAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        public async Task<SystemAccount?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            // Lấy account từ DB bằng email
            var account = await _accountService.GetAccountByEmailAsync(email);
            if (account == null)
            {
                return null;
            }

            // Kiểm tra account có thể login không
            if (!CanLogin(account))
            {
                return null;
            }

            // Verify password
            if (!VerifyPassword(password, account.AccountPassword))
            {
                return null;
            }

            return account;
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        public bool HasRole(SystemAccount account, int requiredRole)
        {
            if (account == null || !account.AccountRole.HasValue)
            {
                return false;
            }

            // Admin có tất cả quyền
            if (account.AccountRole.Value == UserRoles.Admin)
            {
                return true;
            }

            return account.AccountRole.Value == requiredRole;
        }

        /// <summary>
        /// Validate if account is active and can login
        /// </summary>
        public bool CanLogin(SystemAccount account)
        {
            if (account == null)
            {
                return false;
            }

            // Model SystemAccount không có field AccountStatus, nên mặc định tất cả account hợp lệ có thể login
            // Nếu sau này thêm status, có thể cập nhật logic ở đây (ví dụ: return account.AccountStatus == 1;)
            return true;
        }

        /// <summary>
        /// Hash password (sử dụng BCrypt)
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verify password hash
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}