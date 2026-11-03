using Microsoft.AspNetCore.Mvc;
using RaceDay_POE_Web.Filters;
using RaceDay_POE_Web.Helpers;

namespace RaceDay_POE_Web.Controllers;

// Organiser area - only signed-in Organisers can reach these pages
[RequireRole(RoleNames.Organiser)]
public class OrganiserController : Controller
{
    public IActionResult Dashboard()
    {
        ViewData["FirstName"] = HttpContext.Session.GetString(SessionKeys.FirstName);
        return View();
    }
}
