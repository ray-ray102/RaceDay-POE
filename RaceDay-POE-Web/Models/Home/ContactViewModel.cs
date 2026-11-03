using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_Web.Models.Home;

public class ContactViewModel
{
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a subject.")]
    [StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a message.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Your message must be between 10 and 2000 characters.")]
    public string Message { get; set; } = string.Empty;
}
