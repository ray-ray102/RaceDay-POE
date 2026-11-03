using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay_POE_Web.Helpers;

namespace RaceDay_POE_Web.Filters;

// Session-based access control: signed-out users go to Login (with a returnUrl),
// signed-in users without one of the allowed roles go to Account/AccessDenied
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireLoginAttribute : ActionFilterAttribute
{
    public string[] Roles { get; }

    public RequireLoginAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var http = context.HttpContext;
        if (http.Session.GetInt32(SessionKeys.UserId) == null)
        {
            var returnUrl = http.Request.Path + http.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        if (Roles.Length > 0)
        {
            var role = http.Session.GetString(SessionKeys.Role);
            if (role == null || !Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}

// Shortcut for actions that only one role may use, e.g. [RequireRole(RoleNames.Organiser)]
public class RequireRoleAttribute : RequireLoginAttribute
{
    public RequireRoleAttribute(params string[] roles) : base(roles)
    {
    }
}
