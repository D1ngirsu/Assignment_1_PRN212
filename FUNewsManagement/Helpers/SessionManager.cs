using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace FUNewsManagement.Helpers
{
    public static class SessionManager
    {
        private const string USER_SESSION_KEY = "CurrentUser";
        private const string USER_ID_KEY = "UserId";
        private const string USER_EMAIL_KEY = "UserEmail";
        private const string USER_NAME_KEY = "UserName";
        private const string USER_ROLE_KEY = "UserRole";

        /// <summary>
        /// Set current user to session
        /// </summary>
        public static void SetCurrentUser(this ISession session, SystemAccount user)
        {
            if (user == null)
                return;

            // Lưu user object dạng JSON
            var userJson = JsonSerializer.Serialize(user);
            session.SetString(USER_SESSION_KEY, userJson);

            // Lưu thêm các thông tin riêng lẻ để dễ truy xuất
            session.SetInt32(USER_ID_KEY, user.AccountId);
            session.SetString(USER_EMAIL_KEY, user.AccountEmail ?? "");
            session.SetString(USER_NAME_KEY, user.AccountName ?? "");
            session.SetInt32(USER_ROLE_KEY, user.AccountRole ?? 0);
        }

        /// <summary>
        /// Get current user from session
        /// </summary>
        public static SystemAccount? GetCurrentUser(this ISession session)
        {
            var userJson = session.GetString(USER_SESSION_KEY);
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<SystemAccount>(userJson);
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        public static short? GetCurrentUserId(this ISession session)
        {
            var userId = session.GetInt32(USER_ID_KEY);
            return userId.HasValue ? (short)userId.Value : null;
        }

        /// <summary>
        /// Get current user email
        /// </summary>
        public static string? GetCurrentUserEmail(this ISession session)
        {
            return session.GetString(USER_EMAIL_KEY);
        }

        /// <summary>
        /// Get current user name
        /// </summary>
        public static string? GetCurrentUserName(this ISession session)
        {
            return session.GetString(USER_NAME_KEY);
        }

        /// <summary>
        /// Get current user role
        /// </summary>
        public static int? GetCurrentUserRole(this ISession session)
        {
            return session.GetInt32(USER_ROLE_KEY);
        }

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetInt32(USER_ID_KEY).HasValue;
        }

        /// <summary>
        /// Check if current user has specific role
        /// </summary>
        public static bool HasRole(this ISession session, int requiredRole)
        {
            var userRole = session.GetInt32(USER_ROLE_KEY);
            if (!userRole.HasValue)
                return false;

            // Admin có tất cả quyền
            if (userRole.Value == 1)
                return true;

            return userRole.Value == requiredRole;
        }

        /// <summary>
        /// Check if current user is Admin
        /// </summary>
        public static bool IsAdmin(this ISession session)
        {
            return session.HasRole(1);
        }

        /// <summary>
        /// Clear session (logout)
        /// </summary>
        public static void ClearSession(this ISession session)
        {
            session.Clear();
        }
    }
}