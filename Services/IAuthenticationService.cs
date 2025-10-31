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
    }

    /// <summary>
    /// User roles definition
    /// </summary>
    public static class UserRoles
    {
        public const int Staff = 1;
        public const int Lecturer = 2;
        public const int Admin = 3;

        public static string GetRoleName(int role)
        {
            return role switch
            {
                3 => "Admin",
                1 => "Staff",
                2 => "Lecturer",
                _ => "Unknown"
            };
        }
    }
}
