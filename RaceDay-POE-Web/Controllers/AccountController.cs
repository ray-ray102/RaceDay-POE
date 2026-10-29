using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDay_POE_Web.Helpers;
using RaceDay_POE_Web.Models.Account;
using RaceDay_POE_Web.Models.Api;
using RaceDay_POE_Web.Services;

namespace RaceDay_POE_Web.Controllers;

// Register, login and logout - all done through the RaceDay Web API
public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;

    public AccountController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register()
    {
        LoadRoles();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            LoadRoles();
            return View(model);
        }

        var request = new RegisterRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password,
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber,
            Role = model.Role
        };

        HttpResponseMessage response;
        try
        {
            response = await _api.PostAsync("api/auth/register", request);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The RaceDay API is not reachable. Please try again later.");
            LoadRoles();
            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadMessageAsync(response);
            ModelState.AddModelError(string.Empty, error ?? "Registration failed. Please check your details.");
            LoadRoles();
            return View(model);
        }

        TempData["Success"] = "Account created. You can now log in.";
        return RedirectToAction("Index", "Home");
    }

    private void LoadRoles()
    {
        ViewBag.Roles = new SelectList(RoleNames.All);
    }

    private static async Task<string?> ReadMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiMessage>();
            return body?.Message;
        }
        catch
        {
            return null;
        }
    }
}
