using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.DTOs
{
    // Data sent by the client when logging in
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}