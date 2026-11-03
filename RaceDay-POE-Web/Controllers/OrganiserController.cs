using Microsoft.AspNetCore.Mvc;
using RaceDay_POE_Web.Filters;
using RaceDay_POE_Web.Helpers;
using RaceDay_POE_Web.Models.Api;
using RaceDay_POE_Web.Services;

namespace RaceDay_POE_Web.Controllers;

// Organiser area - only signed-in Organisers can reach these pages
[RequireRole(RoleNames.Organiser)]
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    public IActionResult Dashboard()
    {
        ViewData["FirstName"] = HttpContext.Session.GetString(SessionKeys.FirstName);
        return View();
    }

    // GET: /Organiser/Events - the organiser's own events from GET api/events/mine
    public async Task<IActionResult> Events()
    {
        var events = await _api.GetAsync<List<OrganiserEventDto>>("api/events/mine");
        if (events == null)
        {
            ViewData["ApiError"] = "We couldn't load your events from the API. Please try again.";
            events = new List<OrganiserEventDto>();
        }
        return View(events);
    }
}
