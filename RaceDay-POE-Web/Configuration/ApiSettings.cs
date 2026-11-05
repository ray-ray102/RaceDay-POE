namespace RaceDay_POE_Web.Configuration;

/// <summary>Settings for reaching the RaceDay Part 2 Web API (bound from "ApiSettings").</summary>
public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    /// <summary>Base URL of the RaceDay Web API, e.g. http://localhost:5141/</summary>
    public string BaseUrl { get; set; } = string.Empty;
}
