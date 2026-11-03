using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay_POE_Web.Filters;
using RaceDay_POE_Web.Helpers;
using RaceDay_POE_Web.Models.Api;
using RaceDay_POE_Web.Models.Organiser;
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

    // GET: /Organiser/Dashboard - summary counts come from GET api/events/mine
    public async Task<IActionResult> Dashboard()
    {
        ViewData["FirstName"] = HttpContext.Session.GetString(SessionKeys.FirstName);
        var events = await _api.GetAsync<List<OrganiserEventDto>>("api/events/mine") ?? new List<OrganiserEventDto>();
        return View(events);
    }

    // GET: /Organiser/Events - the organiser's own events
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

    // GET: /Organiser/Create
    public IActionResult Create()
    {
        return View("EventForm", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30).AddHours(7) });
    }

    // POST: /Organiser/Create -> POST api/events
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        model.EventId = null;
        if (!ModelState.IsValid)
        {
            return View("EventForm", model);
        }

        var response = await _api.PostAsync("api/events", model.ToRequest());
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"\"{model.Name}\" was created.";
            return RedirectToAction(nameof(Events));
        }

        await AddApiErrorsAsync(response, "Could not create the event.");
        return View("EventForm", model);
    }

    // GET: /Organiser/Edit/5 -> GET api/events/5
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _api.GetAsync<EventResponseDto>($"api/events/{id}");
        if (dto == null)
        {
            return NotFound();
        }
        // Only the owner may edit - the API also enforces this on PUT
        if (dto.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        return View("EventForm", EventFormViewModel.FromDto(dto));
    }

    // POST: /Organiser/Edit/5 -> PUT api/events/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        model.EventId = id;
        if (!ModelState.IsValid)
        {
            return View("EventForm", model);
        }

        var response = await _api.PutAsync($"api/events/{id}", model.ToRequest());
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"\"{model.Name}\" was updated.";
            return RedirectToAction(nameof(Events));
        }
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        await AddApiErrorsAsync(response, "Could not update the event.");
        return View("EventForm", model);
    }

    // Copies validation errors / messages from the API response into ModelState
    private async Task AddApiErrorsAsync(HttpResponseMessage response, string fallback)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in errors.EnumerateObject())
                    foreach (var msg in field.Value.EnumerateArray())
                        ModelState.AddModelError(string.Empty, msg.GetString() ?? fallback);
                return;
            }
            if (root.TryGetProperty("message", out var message))
            {
                ModelState.AddModelError(string.Empty, message.GetString() ?? fallback);
                return;
            }
        }
        catch (JsonException)
        {
            // Not JSON - fall through to the generic message
        }
        ModelState.AddModelError(string.Empty, fallback);
    }
}
