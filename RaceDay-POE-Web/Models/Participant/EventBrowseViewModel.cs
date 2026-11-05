using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Participant;

// Filter form + results for Browse Events (GET api/events)
public class EventBrowseViewModel
{
    public string? EventType { get; set; }
    public string? Location { get; set; }
    public DateTime? FromDate { get; set; }
    public List<EventResponseDto> Events { get; set; } = new();
    public string? ApiError { get; set; }
}
