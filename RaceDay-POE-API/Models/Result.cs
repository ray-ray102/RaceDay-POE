using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_POE_API.Models
{
    // The finish time and position captured by the Organiser after the event
    public class Result
    {
        [Key]
        public int ResultsId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        // Has to be greater than zero
        [Required]
        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }

        [ForeignKey(nameof(EnrolmentId))]
        public Enrolment? Enrolment { get; set; }
    }
}