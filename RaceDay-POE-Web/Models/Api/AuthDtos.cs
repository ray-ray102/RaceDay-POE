namespace RaceDay_POE_Web.Models.Api;

// Mirrors RaceDay-POE-API DTOs/RegisterDto.cs (POST api/auth/register)
public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
}

// Mirrors DTOs/LoginDto.cs (POST api/auth/login)
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Mirrors DTOs/AuthResponseDto.cs, returned by register and login
public class AuthResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// Error body the API returns, e.g. { "message": "This email is already in use." }
public class ApiMessage
{
    public string? Message { get; set; }
}
