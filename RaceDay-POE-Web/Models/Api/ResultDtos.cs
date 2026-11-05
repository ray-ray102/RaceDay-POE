namespace RaceDay_POE_Web.Models.Api;

// Mirrors ResultRequestDto (POST/PUT results)
public class ResultRequestDto
{
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
}

// Mirrors ResultResponseDto
public class ResultResponseDto
{
    public int ResultsId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
}

// Mirrors ResultDetailDto (GET api/events/{id}/results, GET api/results/mine)
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
