using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay_POE_Web.Filters;
using RaceDay_POE_Web.Helpers;
using RaceDay_POE_Web.Models.Api;
using RaceDay_POE_Web.Models.Participant;
using RaceDay_POE_Web.Services;

namespace RaceDay_POE_Web.Controllers;

// Participant area - only signed-in Participants can reach these pages
[RequireRole(RoleNames.Participant)]
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    // GET: /Participant/Dashboard
    public IActionResult Dashboard()
    {
        ViewData["FirstName"] = HttpContext.Session.GetString(SessionKeys.FirstName);
        return View();
    }

    // GET: /Participant/Events -> GET api/events?eventType=&location=&fromDate=
    public async Task<IActionResult> Events(string? eventType, string? location, DateTime? fromDate)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(eventType))
            query.Add($"eventType={Uri.EscapeDataString(eventType.Trim())}");
        if (!string.IsNullOrWhiteSpace(location))
            query.Add($"location={Uri.EscapeDataString(location.Trim())}");
        if (fromDate.HasValue)
            query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

        var path = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var events = await _api.GetAsync<List<EventResponseDto>>(path);

        var model = new EventBrowseViewModel
        {
            EventType = eventType,
            Location = location,
            FromDate = fromDate,
            Events = events ?? new List<EventResponseDto>(),
            ApiError = events == null ? "We couldn't load upcoming events from the API. Please try again." : null
        };
        return View(model);
    }

    // GET: /Participant/EventDetails/5 -> GET api/events/5 + GET api/enrolments/mine
    public async Task<IActionResult> EventDetails(int id)
    {
        var ev = await _api.GetAsync<EventResponseDto>($"api/events/{id}");
        if (ev == null)
        {
            return NotFound();
        }

        var enrolments = await _api.GetAsync<List<EnrolmentResponseDto>>("api/enrolments/mine")
            ?? new List<EnrolmentResponseDto>();
        var mine = enrolments.FirstOrDefault(e => e.EventId == id);

        return View(new EventDetailsViewModel
        {
            Event = ev,
            AlreadyEnrolled = mine != null,
            ExistingStatus = mine?.Status
        });
    }

    // POST: /Participant/Enrol -> POST api/enrolments
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int id, EventDetailsViewModel model)
    {
        var ev = await _api.GetAsync<EventResponseDto>($"api/events/{id}");
        if (ev == null)
        {
            return NotFound();
        }

        model.Event = ev;
        if (!model.CategoryId.HasValue || model.CategoryId.Value <= 0)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Choose a category to enter.");
        }

        if (!ModelState.IsValid)
        {
            return View("EventDetails", model);
        }

        var response = await _api.PostAsync("api/enrolments", new EnrolRequestDto
        {
            EventId = id,
            CategoryId = model.CategoryId!.Value
        });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"You are entered for \"{ev.Name}\".";
            return RedirectToAction(nameof(Enrolments));
        }

        await AddApiErrorsAsync(response, "Could not enrol for this event.");
        // Reload enrolment state for the form
        var enrolments = await _api.GetAsync<List<EnrolmentResponseDto>>("api/enrolments/mine")
            ?? new List<EnrolmentResponseDto>();
        var mine = enrolments.FirstOrDefault(e => e.EventId == id);
        model.AlreadyEnrolled = mine != null;
        model.ExistingStatus = mine?.Status;
        return View("EventDetails", model);
    }

    // GET: /Participant/Enrolments -> GET api/enrolments/mine
    public async Task<IActionResult> Enrolments()
    {
        var enrolments = await _api.GetAsync<List<EnrolmentResponseDto>>("api/enrolments/mine");
        if (enrolments == null)
        {
            ViewData["ApiError"] = "We couldn't load your enrolments from the API. Please try again.";
            enrolments = new List<EnrolmentResponseDto>();
        }
        return View(enrolments);
    }

    // GET: /Participant/Results -> GET api/results/mine
    public async Task<IActionResult> Results()
    {
        var results = await _api.GetAsync<List<ResultDetailDto>>("api/results/mine");
        if (results == null)
        {
            ViewData["ApiError"] = "We couldn't load your results from the API. Please try again.";
            results = new List<ResultDetailDto>();
        }
        return View(results);
    }

    // GET: /Participant/Profile -> GET api/users/me
    public async Task<IActionResult> Profile()
    {
        var profile = await _api.GetAsync<UserProfileDto>("api/users/me");
        if (profile == null)
        {
            ViewData["ApiError"] = "We couldn't load your profile from the API. Please try again.";
            return View(new ProfileFormViewModel
            {
                FirstName = HttpContext.Session.GetString(SessionKeys.FirstName) ?? string.Empty,
                Role = HttpContext.Session.GetString(SessionKeys.Role) ?? RoleNames.Participant
            });
        }
        return View(ProfileFormViewModel.FromDto(profile));
    }

    // POST: /Participant/Profile -> PUT api/users/me
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileFormViewModel model)
    {
        // Email / role are display-only; restore them if the form posts blanks
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Role))
        {
            var current = await _api.GetAsync<UserProfileDto>("api/users/me");
            if (current != null)
            {
                model.Email = current.Email;
                model.Role = current.Role;
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _api.PutAsync("api/users/me", model.ToRequest());
        if (response.IsSuccessStatusCode)
        {
            var updated = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            if (updated != null)
            {
                HttpContext.Session.SetString(SessionKeys.FirstName, updated.FirstName);
                model = ProfileFormViewModel.FromDto(updated);
            }
            TempData["Success"] = "Your profile was updated.";
            return View(model);
        }

        await AddApiErrorsAsync(response, "Could not update your profile.");
        return View(model);
    }

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
            // Not JSON - fall through
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            ModelState.AddModelError(string.Empty, "You are not allowed to do that.");
            return;
        }
        ModelState.AddModelError(string.Empty, fallback);
    }
}
