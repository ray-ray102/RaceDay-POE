using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_POE_API.Models
{
    // A race event, each event is owned by one Organiser
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

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

        [Column(TypeName = "decimal(6,2)")]
        public decimal? DistanceKm { get; set; }

        // Must be Run, Walk or Cycle
        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? BannerImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(OrganiserId))]
        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}