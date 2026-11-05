using System.ComponentModel.DataAnnotations;
using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Participant;

// Profile edit form - picture file upload waits on Blob Storage
public class ProfileFormViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [MaxLength(300)]
    [Display(Name = "Profile picture URL")]
    public string? ProfilePictureUrl { get; set; }

    public string Role { get; set; } = string.Empty;

    public UpdateProfileDto ToRequest() => new()
    {
        FirstName = FirstName.Trim(),
        LastName = LastName.Trim(),
        PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
        ProfilePictureUrl = string.IsNullOrWhiteSpace(ProfilePictureUrl) ? null : ProfilePictureUrl.Trim()
    };

    public static ProfileFormViewModel FromDto(UserProfileDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        ProfilePictureUrl = dto.ProfilePictureUrl,
        Role = dto.Role
    };
}
