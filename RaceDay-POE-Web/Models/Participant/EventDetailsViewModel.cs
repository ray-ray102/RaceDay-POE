using System.ComponentModel.DataAnnotations;
using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Participant;

// Event detail page with category picker for enrolment
public class EventDetailsViewModel
{
    public EventResponseDto Event { get; set; } = new();

    [Required(ErrorMessage = "Choose a category to enter.")]
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    public bool AlreadyEnrolled { get; set; }
    public string? ExistingStatus { get; set; }
}
