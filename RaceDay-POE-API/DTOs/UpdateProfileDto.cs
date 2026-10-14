using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.DTOs
{
    // Fields a user is allowed to change, email and role cannot be changed here
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? ProfilePictureUrl { get; set; }
    }
}