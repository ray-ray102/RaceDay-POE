using System.ComponentModel.DataAnnotations;
using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Organiser;

public class ResultEntryRow
{
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? ResultsId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
}

public class ResultCaptureForm
{
    public int EnrolmentId { get; set; }

    [Required(ErrorMessage = "Finish time is required.")]
    [Display(Name = "Finish time")]
    [RegularExpression(@"^\d{1,2}:\d{2}:\d{2}$", ErrorMessage = "Use hh:mm:ss, e.g. 00:52:18.")]
    public string FinishTime { get; set; } = "00:00:00";

    [Required(ErrorMessage = "Finish position is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Finish position must be greater than zero.")]
    [Display(Name = "Position")]
    public int? FinishPosition { get; set; }
}

public class ResultsPageViewModel
{
    public EventResponseDto Event { get; set; } = new();
    public List<ResultEntryRow> Rows { get; set; } = new();
}
