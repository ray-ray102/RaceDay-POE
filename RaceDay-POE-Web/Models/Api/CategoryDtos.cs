namespace RaceDay_POE_Web.Models.Api;

// Mirrors CategoryRequestDto (POST/PUT categories)
public class CategoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? DistanceKm { get; set; }
}

// Mirrors CategoryResponseDto
public class CategoryResponseDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? DistanceKm { get; set; }
}
