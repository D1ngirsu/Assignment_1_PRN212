using FUNewsManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FUNewsManagement.Filters
{
    /// <summary>
    /// Require user to be logged in
    /// </summary>
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!session.IsLoggedIn())
            {
                // Redirect to login with return URL
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    new { returnUrl = returnUrl });
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Require specific role
    /// </summary>
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly int[] _requiredRoles;

        public AuthorizeRoleAttribute(params int[] requiredRoles)
        {
            _requiredRoles = requiredRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Check if logged in
            if (!session.IsLoggedIn())
            {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    new { returnUrl = returnUrl });
                return;
            }

            // Check role
            var userRole = session.GetCurrentUserRole();
            if (!userRole.HasValue)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            // Admin has all permissions
            if (userRole.Value == 1)
            {
                base.OnActionExecuting(context);
                return;
            }

            // Check if user has required role
            if (!_requiredRoles.Contains(userRole.Value))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Require Admin role only
    /// </summary>
    public class AdminOnlyAttribute : AuthorizeRoleAttribute
    {
        public AdminOnlyAttribute() : base(1)
        {
        }
    }

    /// <summary>
    /// Require Staff role (Staff + Admin)
    /// </summary>
    public class StaffOnlyAttribute : AuthorizeRoleAttribute
    {
        public StaffOnlyAttribute() : base(1, 2)
        {
        }
    }
}