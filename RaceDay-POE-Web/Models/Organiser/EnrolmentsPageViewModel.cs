using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Organiser;

public class EnrolmentsPageViewModel
{
    public EventResponseDto Event { get; set; } = new();
    public List<EnrolmentResponseDto> Enrolments { get; set; } = new();
}
