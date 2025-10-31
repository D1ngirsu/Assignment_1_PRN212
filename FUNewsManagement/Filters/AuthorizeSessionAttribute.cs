using FUNewsManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services;

namespace FUNewsManagement.Filters
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!session.IsLoggedIn())
            {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = returnUrl });
            }

            base.OnActionExecuting(context);
        }
    }

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

            if (!session.IsLoggedIn())
            {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = returnUrl });
                return;
            }

            var userRole = session.GetCurrentUserRole();
            if (!userRole.HasValue)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            if (userRole.Value == UserRoles.Admin)
            {
                base.OnActionExecuting(context);
                return;
            }

            if (!_requiredRoles.Contains(userRole.Value))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    public class AdminOnlyAttribute : AuthorizeRoleAttribute
    {
        public AdminOnlyAttribute() : base(UserRoles.Admin) { }
    }

    public class StaffOnlyAttribute : AuthorizeRoleAttribute
    {
        public StaffOnlyAttribute() : base(UserRoles.Admin, UserRoles.Staff) { }
    }
}
