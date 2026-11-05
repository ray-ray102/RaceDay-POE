using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_POE_API.Models
{
    // Links a Participant to an event and the category they picked
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        // Must be Pending, Confirmed or Cancelled
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [ForeignKey(nameof(ParticipantId))]
        public User? Participant { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // An enrolment can have at most one result
        public Result? Result { get; set; }
    }
}