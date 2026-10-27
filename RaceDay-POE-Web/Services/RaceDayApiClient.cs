using System.Net.Http.Json;

namespace RaceDay_POE_Web.Services;

/// <summary>
/// Typed HttpClient wrapper for the RaceDay Part 2 Web API.
/// All data access in the MVC app goes through this client - no direct database access.
/// </summary>
public class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<RaceDayApiClient> _logger;

    public RaceDayApiClient(HttpClient http, ILogger<RaceDayApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(path, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GET {Path} returned {Status}", path, (int)response.StatusCode);
            return default;
        }
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    public Task<HttpResponseMessage> PostAsync<T>(string path, T body, CancellationToken ct = default)
        => _http.PostAsJsonAsync(path, body, ct);

    public Task<HttpResponseMessage> PutAsync<T>(string path, T body, CancellationToken ct = default)
        => _http.PutAsJsonAsync(path, body, ct);

    public Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken ct = default)
        => _http.DeleteAsync(path, ct);
}
