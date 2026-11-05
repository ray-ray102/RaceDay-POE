using System.Globalization;
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
        var dto = await LoadOwnedEventAsync(id);
        if (dto == null)
        {
            return NotFound();
        }
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

    // POST: /Organiser/UploadBanner/5 -> POST api/events/5/banner
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile? bannerFile)
    {
        if (bannerFile == null || bannerFile.Length == 0)
        {
            TempData["Error"] = "Please choose an image file to upload.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var response = await _api.PostMultipartAsync($"api/events/{id}/banner", bannerFile);
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Banner image uploaded.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return RedirectToAction("AccessDenied", "Account");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        TempData["Error"] = await ReadApiMessageAsync(response) ?? "Could not upload the banner image.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // GET: /Organiser/Delete/5 - confirmation page
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await LoadOwnedEventAsync(id);
        if (dto == null)
        {
            return NotFound();
        }
        if (dto.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        return View(dto);
    }

    // POST: /Organiser/Delete/5 -> DELETE api/events/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _api.DeleteAsync($"api/events/{id}");
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
        {
            TempData["Success"] = "Event deleted.";
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

        var message = await ReadApiMessageAsync(response) ?? "Could not delete the event. It may still have enrolments.";
        TempData["Error"] = message;
        return RedirectToAction(nameof(Delete), new { id });
    }

    // GET: /Organiser/Categories/5
    public async Task<IActionResult> Categories(int id)
    {
        var ev = await LoadOwnedEventAsync(id);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return RedirectToAction("AccessDenied", "Account");

        var categories = await _api.GetAsync<List<CategoryResponseDto>>($"api/events/{id}/categories")
            ?? new List<CategoryResponseDto>();

        return View(new CategoriesPageViewModel
        {
            Event = ev,
            Categories = categories,
            Form = new CategoryFormViewModel { EventId = id, EventName = ev.Name }
        });
    }

    // POST: /Organiser/AddCategory/5 -> POST api/events/5/categories
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int id, CategoryFormViewModel form)
    {
        var ev = await LoadOwnedEventAsync(id);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return RedirectToAction("AccessDenied", "Account");

        form.EventId = id;
        form.EventName = ev.Name;
        form.CategoryId = null;

        if (!ModelState.IsValid)
        {
            var categories = await _api.GetAsync<List<CategoryResponseDto>>($"api/events/{id}/categories")
                ?? new List<CategoryResponseDto>();
            return View("Categories", new CategoriesPageViewModel { Event = ev, Categories = categories, Form = form });
        }

        var response = await _api.PostAsync($"api/events/{id}/categories", form.ToRequest());
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Category \"{form.Name}\" was added.";
            return RedirectToAction(nameof(Categories), new { id });
        }

        await AddApiErrorsAsync(response, "Could not add the category.");
        var list = await _api.GetAsync<List<CategoryResponseDto>>($"api/events/{id}/categories")
            ?? new List<CategoryResponseDto>();
        return View("Categories", new CategoriesPageViewModel { Event = ev, Categories = list, Form = form });
    }

    // GET: /Organiser/EditCategory/5
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _api.GetAsync<CategoryResponseDto>($"api/categories/{id}");
        if (category == null) return NotFound();

        var ev = await LoadOwnedEventAsync(category.EventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return RedirectToAction("AccessDenied", "Account");

        return View(CategoryFormViewModel.FromDto(category, ev.Name));
    }

    // POST: /Organiser/EditCategory/5 -> PUT api/categories/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, CategoryFormViewModel form)
    {
        form.CategoryId = id;
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var response = await _api.PutAsync($"api/categories/{id}", form.ToRequest());
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Category \"{form.Name}\" was updated.";
            return RedirectToAction(nameof(Categories), new { id = form.EventId });
        }
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return RedirectToAction("AccessDenied", "Account");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        await AddApiErrorsAsync(response, "Could not update the category.");
        return View(form);
    }

    // POST: /Organiser/DeleteCategory/5 -> DELETE api/categories/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var response = await _api.DeleteAsync($"api/categories/{id}");
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
        {
            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Categories), new { id = eventId });
        }

        TempData["Error"] = await ReadApiMessageAsync(response) ?? "Could not delete the category. It may still have enrolments.";
        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    // GET: /Organiser/Enrolments/5
    public async Task<IActionResult> Enrolments(int id)
    {
        var ev = await LoadOwnedEventAsync(id);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return RedirectToAction("AccessDenied", "Account");

        var enrolments = await _api.GetAsync<List<EnrolmentResponseDto>>($"api/events/{id}/enrolments");
        if (enrolments == null)
        {
            ViewData["ApiError"] = "We couldn't load enrolments from the API. Please try again.";
            enrolments = new List<EnrolmentResponseDto>();
        }

        return View(new EnrolmentsPageViewModel { Event = ev, Enrolments = enrolments });
    }

    // POST: /Organiser/UpdateEnrolmentStatus -> PUT api/enrolments/{id}/status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, int eventId, string status)
    {
        if (status is not ("Confirmed" or "Cancelled"))
        {
            TempData["Error"] = "Status must be Confirmed or Cancelled.";
            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }

        var response = await _api.PutAsync($"api/enrolments/{enrolmentId}/status", new EnrolmentStatusDto { Status = status });
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Enrolment marked as {status}.";
            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }

        TempData["Error"] = await ReadApiMessageAsync(response) ?? "Could not update enrolment status.";
        return RedirectToAction(nameof(Enrolments), new { id = eventId });
    }

    // GET: /Organiser/Results/5
    public async Task<IActionResult> Results(int id)
    {
        var page = await BuildResultsPageAsync(id);
        if (page == null) return NotFound();
        if (page.Event.OrganiserId != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return RedirectToAction("AccessDenied", "Account");
        return View(page);
    }

    // POST: /Organiser/CaptureResult -> POST api/enrolments/{id}/results
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(int eventId, ResultCaptureForm form)
    {
        if (!TryParseFinishTime(form.FinishTime, out var finishTime))
        {
            ModelState.AddModelError(nameof(form.FinishTime), "Use hh:mm:ss, e.g. 00:52:18.");
        }
        if (!form.FinishPosition.HasValue || form.FinishPosition < 1)
        {
            ModelState.AddModelError(nameof(form.FinishPosition), "Finish position must be greater than zero.");
        }

        if (!ModelState.IsValid)
        {
            var page = await BuildResultsPageAsync(eventId);
            if (page == null) return NotFound();
            ViewData["CaptureEnrolmentId"] = form.EnrolmentId;
            ViewData["CaptureFinishTime"] = form.FinishTime;
            ViewData["CaptureFinishPosition"] = form.FinishPosition;
            return View("Results", page);
        }

        var response = await _api.PostAsync($"api/enrolments/{form.EnrolmentId}/results", new ResultRequestDto
        {
            FinishTime = finishTime,
            FinishPosition = form.FinishPosition!.Value
        });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Result captured.";
            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        TempData["Error"] = await ReadApiMessageAsync(response) ?? "Could not capture the result.";
        return RedirectToAction(nameof(Results), new { id = eventId });
    }

    // POST: /Organiser/UpdateResult -> PUT api/results/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateResult(int eventId, int resultsId, ResultCaptureForm form)
    {
        if (!TryParseFinishTime(form.FinishTime, out var finishTime) || !form.FinishPosition.HasValue || form.FinishPosition < 1)
        {
            TempData["Error"] = "Enter a valid finish time (hh:mm:ss) and position.";
            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        var response = await _api.PutAsync($"api/results/{resultsId}", new ResultRequestDto
        {
            FinishTime = finishTime,
            FinishPosition = form.FinishPosition.Value
        });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Result updated.";
            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        TempData["Error"] = await ReadApiMessageAsync(response) ?? "Could not update the result.";
        return RedirectToAction(nameof(Results), new { id = eventId });
    }

    // POST: /Organiser/DeleteResult -> DELETE api/results/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResult(int eventId, int resultsId)
    {
        var response = await _api.DeleteAsync($"api/results/{resultsId}");
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
        {
            TempData["Success"] = "Result removed.";
            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        TempData["Error"] = await ReadApiMessageAsync(response) ?? "Could not delete the result.";
        return RedirectToAction(nameof(Results), new { id = eventId });
    }

    private async Task<EventResponseDto?> LoadOwnedEventAsync(int id)
        => await _api.GetAsync<EventResponseDto>($"api/events/{id}");

    private async Task<ResultsPageViewModel?> BuildResultsPageAsync(int eventId)
    {
        var ev = await LoadOwnedEventAsync(eventId);
        if (ev == null) return null;

        var enrolments = await _api.GetAsync<List<EnrolmentResponseDto>>($"api/events/{eventId}/enrolments")
            ?? new List<EnrolmentResponseDto>();
        var results = await _api.GetAsync<List<ResultDetailDto>>($"api/events/{eventId}/results")
            ?? new List<ResultDetailDto>();
        var byEnrolment = results.ToDictionary(r => r.EnrolmentId);

        var rows = enrolments
            .OrderBy(e => e.ParticipantName)
            .Select(e =>
            {
                byEnrolment.TryGetValue(e.EnrolmentId, out var result);
                return new ResultEntryRow
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantName = e.ParticipantName,
                    CategoryName = e.CategoryName,
                    Status = e.Status,
                    ResultsId = result?.ResultsId,
                    FinishTime = result?.FinishTime,
                    FinishPosition = result?.FinishPosition
                };
            }).ToList();

        return new ResultsPageViewModel { Event = ev, Rows = rows };
    }

    private static bool TryParseFinishTime(string? value, out TimeSpan finishTime)
    {
        finishTime = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        return TimeSpan.TryParseExact(value.Trim(), @"h\:mm\:ss", CultureInfo.InvariantCulture, out finishTime)
            || TimeSpan.TryParseExact(value.Trim(), @"hh\:mm\:ss", CultureInfo.InvariantCulture, out finishTime)
            || TimeSpan.TryParse(value.Trim(), CultureInfo.InvariantCulture, out finishTime);
    }

    private async Task<string?> ReadApiMessageAsync(HttpResponseMessage response)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("message", out var message))
                return message.GetString();
        }
        catch (JsonException)
        {
        }
        return null;
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
        }
        ModelState.AddModelError(string.Empty, fallback);
    }
}
