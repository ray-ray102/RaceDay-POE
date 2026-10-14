using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.DTOs
{
    // Data sent when creating or updating an event
    public class EventRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 9999.99, ErrorMessage = "Distance must be greater than zero.")]
        public decimal? DistanceKm { get; set; }

        // Only Run, Walk or Cycle are accepted
        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Event type must be Run, Walk or Cycle.")]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? BannerImageUrl { get; set; }
    }

    // Short version of a category, used inside the event detail
    public class CategorySummaryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
    }

    // Event data sent back to the client
    public class EventResponseDto
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public List<CategorySummaryDto> Categories { get; set; } = new List<CategorySummaryDto>();
    }

    // Used on the Organiser dashboard, shows how many people entered each event
    public class OrganiserEventDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int EnrolmentCount { get; set; }
    }
}