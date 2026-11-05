namespace RaceDay_POE_Web.Models.Api;

// Mirrors RaceDay-POE-API DTOs/EventDtos.cs - OrganiserEventDto (GET api/events/mine)
public class OrganiserEventDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int EnrolmentCount { get; set; }

    public bool IsUpcoming => EventDate >= DateTime.Now;
}

// Short category shape nested inside EventResponseDto
public class CategorySummaryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
}

// Mirrors EventResponseDto (GET api/events, GET api/events/{id})
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
    public List<CategorySummaryDto> Categories { get; set; } = new();
}

// Mirrors EventRequestDto (POST api/events, PUT api/events/{id})
public class EventRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
}
