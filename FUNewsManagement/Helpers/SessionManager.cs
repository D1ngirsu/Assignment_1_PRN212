using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Services;

namespace FUNewsManagement.Helpers
{
    public static class SessionManager
    {
        private const string USER_SESSION_KEY = "CurrentUser";
        private const string USER_ID_KEY = "UserId";
        private const string USER_EMAIL_KEY = "UserEmail";
        private const string USER_NAME_KEY = "UserName";
        private const string USER_ROLE_KEY = "UserRole";

        public static void SetCurrentUser(this ISession session, SystemAccount user)
        {
            if (user == null)
                return;

            var userForSession = new SystemAccount
            {
                AccountId = user.AccountId,
                AccountName = user.AccountName,
                AccountEmail = user.AccountEmail,
                AccountRole = user.AccountRole
            };

            var userJson = JsonSerializer.Serialize(userForSession);
            session.SetString(USER_SESSION_KEY, userJson);

            session.SetInt32(USER_ID_KEY, user.AccountId);
            session.SetString(USER_EMAIL_KEY, user.AccountEmail ?? "");
            session.SetString(USER_NAME_KEY, user.AccountName ?? "");
            session.SetInt32(USER_ROLE_KEY, user.AccountRole ?? 0);
        }

        public static SystemAccount? GetCurrentUser(this ISession session)
        {
            var userJson = session.GetString(USER_SESSION_KEY);
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<SystemAccount>(userJson);
        }

        public static short? GetCurrentUserId(this ISession session)
        {
            var userId = session.GetInt32(USER_ID_KEY);
            return userId.HasValue ? (short)userId.Value : null;
        }

        public static string? GetCurrentUserEmail(this ISession session)
        {
            return session.GetString(USER_EMAIL_KEY);
        }

        public static string? GetCurrentUserName(this ISession session)
        {
            return session.GetString(USER_NAME_KEY);
        }

        public static int? GetCurrentUserRole(this ISession session)
        {
            return session.GetInt32(USER_ROLE_KEY);
        }

        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetInt32(USER_ID_KEY).HasValue;
        }

        public static bool HasRole(this ISession session, int requiredRole)
        {
            var userRole = session.GetInt32(USER_ROLE_KEY);
            if (!userRole.HasValue)
                return false;

            if (userRole.Value == UserRoles.Admin)
                return true;

            return userRole.Value == requiredRole;
        }

        public static bool IsAdmin(this ISession session)
        {
            var role = session.GetCurrentUserRole();
            return role == UserRoles.Admin;
        }

        public static void ClearSession(this ISession session)
        {
            session.Clear();
        }
    }
}
