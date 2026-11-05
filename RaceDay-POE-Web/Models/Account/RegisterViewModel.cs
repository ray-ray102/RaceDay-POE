using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_Web.Models.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    [Display(Name = "Phone number (optional)")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a role.")]
    [RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Role must be Organiser or Participant.")]
    [Display(Name = "I am signing up as")]
    public string Role { get; set; } = string.Empty;
}
