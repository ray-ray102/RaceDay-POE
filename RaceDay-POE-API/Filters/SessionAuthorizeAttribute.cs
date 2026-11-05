using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay_POE_API.Helpers;

namespace RaceDay_POE_API.Filters
{
    /*
     Checks the session before an action runs.
     No user id in the session returns 401 Unauthorized.
     A role that is not allowed returns 403 Forbidden.
     Leave the roles empty to allow any logged in user.
    */
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            int? userId = session.GetInt32(SessionKeys.UserId);
            string? role = session.GetString(SessionKeys.Role);

            if (userId == null || role == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new ObjectResult(new { message = "You do not have permission to do this." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}