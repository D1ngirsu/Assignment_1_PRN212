using BCrypt.Net;

namespace Services
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash password using BCrypt
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verify if a password matches a stored hash
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
