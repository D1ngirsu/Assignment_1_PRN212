using BusinessObjects.Models;
using FUNewsManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FUNewsManagement.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// Get current logged in user
        /// </summary>
        protected SystemAccount? CurrentUser
        {
            get => HttpContext.Session.GetCurrentUser();
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        protected short? CurrentUserId
        {
            get => HttpContext.Session.GetCurrentUserId();
        }

        /// <summary>
        /// Get current user email
        /// </summary>
        protected string? CurrentUserEmail
        {
            get => HttpContext.Session.GetCurrentUserEmail();
        }

        /// <summary>
        /// Get current user name
        /// </summary>
        protected string? CurrentUserName
        {
            get => HttpContext.Session.GetCurrentUserName();
        }

        /// <summary>
        /// Get current user role
        /// </summary>
        protected int? CurrentUserRole
        {
            get => HttpContext.Session.GetCurrentUserRole();
        }

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        protected bool IsLoggedIn
        {
            get => HttpContext.Session.IsLoggedIn();
        }

        /// <summary>
        /// Check if current user is Admin
        /// </summary>
        protected bool IsAdmin
        {
            get => HttpContext.Session.IsAdmin();
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        protected bool HasRole(int role)
        {
            return HttpContext.Session.HasRole(role);
        }

        /// <summary>
        /// Pass current user to ViewBag automatically
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Tự động pass user info vào ViewBag
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.CurrentUserName = CurrentUserName;
            ViewBag.CurrentUserEmail = CurrentUserEmail;
            ViewBag.CurrentUserRole = CurrentUserRole;
            ViewBag.IsLoggedIn = IsLoggedIn;
            ViewBag.IsAdmin = IsAdmin;

            base.OnActionExecuting(context);
        }
    }
}