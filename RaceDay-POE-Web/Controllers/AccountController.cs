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
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.GetInt32(SessionKeys.UserId) != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        HttpResponseMessage response;
        try
        {
            response = await _api.PostAsync("api/auth/login", new LoginRequest { Email = model.Email, Password = model.Password });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The RaceDay API is not reachable. Please try again later.");
            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadMessageAsync(response);
            ModelState.AddModelError(string.Empty, error ?? "Email or password is incorrect.");
            return View(model);
        }

        var user = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Unexpected response from the API.");
            return View(model);
        }

        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.FirstName, user.FirstName);
        HttpContext.Session.SetString(SessionKeys.LastName, user.LastName);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        TempData["Success"] = $"Welcome back, {user.FirstName}!";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // End the API session too; ignore failures (e.g. it already expired)
            await _api.PostAsync("api/auth/logout");
        }
        catch (HttpRequestException)
        {
        }

        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction(nameof(Login));
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
