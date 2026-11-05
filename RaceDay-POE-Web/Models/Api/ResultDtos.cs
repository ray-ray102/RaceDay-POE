namespace RaceDay_POE_Web.Models.Api;

// Mirrors ResultDetailDto (GET api/results/mine)
public class ResultDetailDto
{
    public int ResultsId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalFinishers { get; set; }
}
