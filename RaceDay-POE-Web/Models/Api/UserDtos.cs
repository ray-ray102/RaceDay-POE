namespace RaceDay_POE_Web.Models.Api;

// Mirrors UserProfileDto (GET api/users/me)
public class UserProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}

// Mirrors UpdateProfileDto (PUT api/users/me)
public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
