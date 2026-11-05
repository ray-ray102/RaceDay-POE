using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Organiser;

public class CategoriesPageViewModel
{
    public EventResponseDto Event { get; set; } = new();
    public List<CategoryResponseDto> Categories { get; set; } = new();
    public CategoryFormViewModel Form { get; set; } = new();
}
