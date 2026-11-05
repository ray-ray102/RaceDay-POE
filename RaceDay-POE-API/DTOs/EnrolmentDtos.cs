using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.DTOs
{
    // Sent by a Participant to enter an event, the participant id comes from the session
    public class EnrolRequestDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    // Sent when changing the status of an enrolment
    public class EnrolmentStatusDto
    {
        [Required]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Status must be Pending, Confirmed or Cancelled.")]
        public string Status { get; set; } = string.Empty;
    }

    // Enrolment data sent back to the client
    public class EnrolmentResponseDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}