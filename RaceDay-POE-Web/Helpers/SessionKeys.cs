namespace RaceDay_POE_Web.Helpers;

// Values kept in the MVC session for the logged-in user
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string FirstName = "FirstName";
    public const string LastName = "LastName";
    public const string Role = "Role";

    // The API uses its own session cookie (no JWT), so we keep it per user and send it back on each call
    public const string ApiCookie = "ApiCookie";
}
