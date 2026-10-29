using System.Net.Http.Json;
using RaceDay_POE_Web.Helpers;

namespace RaceDay_POE_Web.Services;

/// <summary>
/// Typed HttpClient wrapper for the RaceDay Part 2 Web API.
/// All data access in the MVC app goes through this client - no direct database access.
/// The API authenticates with its own session cookie (it does not issue a JWT), so the
/// cookie for the current user is stored in the MVC session and forwarded on every call.
/// </summary>
public class RaceDayApiClient
{
    private const string ApiCookieName = "RaceDay.Session";

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RaceDayApiClient> _logger;

    public RaceDayApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<RaceDayApiClient> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, path), ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GET {Path} returned {Status}", path, (int)response.StatusCode);
            return default;
        }
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    public Task<HttpResponseMessage> PostAsync<T>(string path, T body, CancellationToken ct = default)
        => SendAsync(new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) }, ct);

    public Task<HttpResponseMessage> PostAsync(string path, CancellationToken ct = default)
        => SendAsync(new HttpRequestMessage(HttpMethod.Post, path), ct);

    public Task<HttpResponseMessage> PutAsync<T>(string path, T body, CancellationToken ct = default)
        => SendAsync(new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) }, ct);

    public Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken ct = default)
        => SendAsync(new HttpRequestMessage(HttpMethod.Delete, path), ct);

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var apiCookie = session?.GetString(SessionKeys.ApiCookie);
        if (!string.IsNullOrEmpty(apiCookie))
        {
            request.Headers.Add("Cookie", $"{ApiCookieName}={apiCookie}");
        }

        var response = await _http.SendAsync(request, ct);

        // Keep the API session cookie when the API hands out a new one (e.g. on login)
        if (session != null && response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                if (cookie.StartsWith(ApiCookieName + "=", StringComparison.Ordinal))
                {
                    var value = cookie.Substring(ApiCookieName.Length + 1).Split(';')[0];
                    session.SetString(SessionKeys.ApiCookie, value);
                }
            }
        }

        return response;
    }
}
