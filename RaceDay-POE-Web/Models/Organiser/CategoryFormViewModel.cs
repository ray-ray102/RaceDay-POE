using System.ComponentModel.DataAnnotations;
using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Organiser;

public class CategoryFormViewModel
{
    public int? CategoryId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Range(0, 120)]
    [Display(Name = "Min age")]
    public int? MinAge { get; set; }

    [Range(0, 120)]
    [Display(Name = "Max age")]
    public int? MaxAge { get; set; }

    [Range(0.01, 9999.99, ErrorMessage = "Distance must be greater than zero.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    public bool IsEdit => CategoryId.HasValue;

    public CategoryRequestDto ToRequest() => new()
    {
        Name = Name.Trim(),
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        MinAge = MinAge,
        MaxAge = MaxAge,
        DistanceKm = DistanceKm
    };

    public static CategoryFormViewModel FromDto(CategoryResponseDto dto, string eventName) => new()
    {
        CategoryId = dto.CategoryId,
        EventId = dto.EventId,
        EventName = eventName,
        Name = dto.Name,
        Description = dto.Description,
        MinAge = dto.MinAge,
        MaxAge = dto.MaxAge,
        DistanceKm = dto.DistanceKm
    };
}
