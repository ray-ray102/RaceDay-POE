using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.DTOs
{
    // Data sent when adding or updating a category
    public class CategoryRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        [Range(0, 120)]
        public int? MinAge { get; set; }

        [Range(0, 120)]
        public int? MaxAge { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Distance must be greater than zero.")]
        public decimal? DistanceKm { get; set; }
    }

    // Category data sent back to the client
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
}