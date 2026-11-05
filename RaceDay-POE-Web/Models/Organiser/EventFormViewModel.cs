using System.ComponentModel.DataAnnotations;
using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Organiser;

// Form model for Create/Edit Event - rules match the API's EventRequestDto
public class EventFormViewModel
{
    public int? EventId { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    [MaxLength(150)]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Date & start time")]
    public DateTime? EventDate { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 9999.99, ErrorMessage = "Distance must be greater than zero.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    [Required(ErrorMessage = "Choose an event type.")]
    [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Event type must be Run, Walk or Cycle.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;

    // Kept as-is on edit; banner uploads come later via Blob Storage
    public string? BannerImageUrl { get; set; }

    public bool IsEdit => EventId.HasValue;

    public EventRequestDto ToRequest() => new()
    {
        Name = Name.Trim(),
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        EventDate = EventDate!.Value,
        Location = Location.Trim(),
        DistanceKm = DistanceKm,
        EventType = EventType,
        BannerImageUrl = BannerImageUrl
    };

    public static EventFormViewModel FromDto(EventResponseDto dto) => new()
    {
        EventId = dto.EventId,
        Name = dto.Name,
        Description = dto.Description,
        EventDate = dto.EventDate,
        Location = dto.Location,
        DistanceKm = dto.DistanceKm,
        EventType = dto.EventType,
        BannerImageUrl = dto.BannerImageUrl
    };
}
