using BusinessObjects.Models;

namespace Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        Task<SystemAccount?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        bool HasRole(SystemAccount account, int requiredRole);

        /// <summary>
        /// Validate if account is active and can login
        /// </summary>
        bool CanLogin(SystemAccount account);

        /// <summary>
        /// Hash password (nên dùng BCrypt hoặc PBKDF2 trong production)
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify password hash
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// User roles definition
    /// </summary>
    public static class UserRoles
    {
        public const int Admin = 1;
        public const int Staff = 2;
        public const int Lecturer = 3;

        public static string GetRoleName(int role)
        {
            return role switch
            {
                1 => "Admin",
                2 => "Staff",
                3 => "Lecturer",
                _ => "Unknown"
            };
        }
    }
}