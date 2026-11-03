using Microsoft.AspNetCore.Mvc;
using RaceDay_POE_Web.Filters;
using RaceDay_POE_Web.Helpers;

namespace RaceDay_POE_Web.Controllers;

// Participant area - only signed-in Participants can reach these pages
[RequireRole(RoleNames.Participant)]
public class ParticipantController : Controller
{
    public IActionResult Dashboard()
    {
        ViewData["FirstName"] = HttpContext.Session.GetString(SessionKeys.FirstName);
        return View();
    }
}
