using Microsoft.AspNetCore.Mvc;
using RaceDay_POE_API.Helpers;

namespace RaceDay_POE_API.Controllers
{
    // Shared helpers so every controller can read the logged in user from the session
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        // Only use these on actions protected by SessionAuthorize
        protected int CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

        protected string CurrentRole => HttpContext.Session.GetString(SessionKeys.Role) ?? string.Empty;
    }
}