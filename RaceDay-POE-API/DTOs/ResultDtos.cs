using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.DTOs
{
    // Sent by the Organiser when capturing or updating a result
    public class ResultRequestDto
    {
        // Format is hh:mm:ss, for example 00:52:18
        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Finish position must be greater than zero.")]
        public int FinishPosition { get; set; }
    }

    // Result data sent back to the client
    public class ResultResponseDto
    {
        public int ResultsId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int FinishPosition { get; set; }
    }

    // Used for leaderboards and a participant's history, includes the names as well
    public class ResultDetailDto : ResultResponseDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}